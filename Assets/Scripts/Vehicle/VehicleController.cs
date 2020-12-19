using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.PlayerInput;

[RequireComponent(typeof(IInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GearboxController))]
public class VehicleController : MonoBehaviour
{
    [Header("Adjustable Parameters")]
    [SerializeField] private float _maxSteeringAngle = 35f;
    [SerializeField] private float _maxBrakingTorque = 4500f;
    [SerializeField] private float _handbrakeTorque = 10000f;
    [SerializeField] [Range(0.001f, 1.0f)] private float _steerSpeed = 0.2f;
    //[SerializeField] private float _shiftUpTime = 1.0f;
    //[SerializeField] private float _shiftDownTime = 0.2f;
    [SerializeField] [Range(0.5f, 600f)] private float _downforce = 1.0f;
    [SerializeField] private float _hybridBoostTorque = 160f;
    [SerializeField] private float _speedThreshold = 10f;

    [Header("Hybrid boost")]
    [SerializeField] private bool _enableHybridBoost = false;
    [SerializeField] private float _hybridBoostDepletionRate = 20f;
    [SerializeField] private float _hybridBoostRechargeRate = 20f;
    [Range(0f, 100f)]
    [SerializeField] private float _hybridBoostRequiredAmount = 80f;

    private IInput _input;
    private Fuel _fuel;
    
    private bool _cutThrottle = false;

    private Rigidbody _myRigidBody;

    private GearboxController _gearboxController;

    [SerializeField] public AxleInfo[] _axleInfos;

    public IEngine Engine;
    public IGearbox Gearbox;

    [SerializeField] private Transform _centerOfMass;

    private float _currentEngineRpm;
    private int _currentGear = 0;
    private float _currentEngineTorque = 0f;
    private float _currentTransmissionTorque = 0.0001f;
    private float _currentBackdriveTorque = 0f;
    private float _currentSpeed = 0.0f;
    private float _currentBoostAmount = 100f;
    private const float _maxBoostAmount = 100f;

    private bool _isHybridBoostApplied = false;
    private bool _isHybridBoostAvailable = true;

    // 1e8f == 100000000

    // Constant parameters
    private float _wheelInertia = 0.92f;
    private const float radiansToRevs = 0.159155f;
    [SerializeField] private float _defaultSidewaysFriction = 1.4f;
    [SerializeField] private float _defaultForwardFriction = 1.4f;
    [SerializeField] private float _lockedBrakesSidewaysFriction = 0.8f;
    [SerializeField] private float _lockedHandBrakeSidewaysFriction = 0.8f;
    [SerializeField] private float _offTrackFriction = 0.3f;

    public float EngineTorque => this._currentEngineTorque;
    public float EngineRpm => this._currentEngineRpm;
    public int CurrentGear => this._currentGear;
    public float BoostAmount => this._currentBoostAmount;
    public bool HybridBoostAvailable => this._isHybridBoostAvailable;
    public bool HybridBoostEnabled => this._enableHybridBoost;
    public float Speed => this._currentSpeed;
    public float SpeedThreshold => this._speedThreshold;
    public bool CutTrottle
    {
        get => this._cutThrottle;
        set => this._cutThrottle = value;
    }
    public AxleInfo[] MotorAxles => this._axleInfos.Where(x => x.Motor).ToArray();

    void Start()
    {
        this._myRigidBody = GetComponent<Rigidbody>();
        this._input = GetComponent<IInput>();
        this._fuel = GetComponent<Fuel>();
        this.Engine = GetComponent<IEngine>();
        this.Gearbox = GetComponent<IGearbox>();

        this._gearboxController = GetComponent<GearboxController>();

        this._currentEngineRpm = this.Engine.MinimumRpm;      

        if (this._myRigidBody != null && this._centerOfMass != null)
        {
            this._myRigidBody.centerOfMass = this._centerOfMass.localPosition;
        }

        foreach (AxleInfo axle in this._axleInfos.Where(x => x.Motor))
        {
            axle.LeftWheel.motorTorque = this._currentTransmissionTorque;
            axle.RightWheel.motorTorque = this._currentTransmissionTorque;
        }
    }

    void FixedUpdate()
    {
        this.ApplyTransmissionTorqueToWheels();
        this.ApplyBrakeTorqueToWheels();
        this.ApplySteeringToWheels();
        this.ApplyHandbrake();
        this.AddDownforce();
        this.AdjustWheelFriction();
        this.DetectWheelSlip();
    }

    void Update()
    {
        this.GetCurrentGear();
        this.RevEngine();
        this.GetCurrentSpeed();
        this.ApplyHybridBoost();
        this.RechargeHybridBoost();
    }

    private void GetCurrentGear()
    {
        this._currentGear = this._gearboxController.Gear;
    }

    private void ApplyTransmissionTorqueToWheels()
    {
        float thrustTorque = 0f;
        float backdriveTorque = 0f;
        float engineBrakeTorque = 0f;

        foreach (AxleInfo axle in this._axleInfos.Where(x => x.Motor))
        {
            if (this._currentGear != 0)
            {
                float currentGearRation = this._currentGear >= 0 ? this.Gearbox.ForwardGearRatios[this._currentGear] : this.Gearbox.ReverseRatio;
                thrustTorque = this._currentTransmissionTorque;
                backdriveTorque = Mathf.Clamp(this._currentBackdriveTorque, 0, 1e8f) / currentGearRation;
                engineBrakeTorque = Mathf.Clamp(this._currentBackdriveTorque, -1e8f, 0f) / currentGearRation;
            }

            float totalTorque = thrustTorque + backdriveTorque;
            float maxWheelRpm = this.GetMaximumWheelRpmPossible();

            if (axle.GetAxleWheels().Any(x => x.rpm >= maxWheelRpm))
            {
                foreach (WheelCollider wheel in axle.GetAxleWheels())
                {
                    wheel.motorTorque = 0.0001f;
                }
            }
            else if (this._fuel.CurrentFuel <= 0f)
            {
                foreach (WheelCollider wheel in axle.GetAxleWheels())
                {
                    wheel.motorTorque = 100f;
                }
            }
            else
            {
                int torqueDirection = this._currentGear >= 0 ? 1 : -1;
                foreach (WheelCollider wheel in axle.GetAxleWheels())
                {
                    wheel.motorTorque = (totalTorque / 2) * torqueDirection * axle.TorqueBias;
                }
            }

            foreach (WheelCollider wheel in axle.GetAxleWheels())
            {
                wheel.brakeTorque += Mathf.Abs(engineBrakeTorque);
            }
        }
    }

    private float GetMaximumWheelRpmPossible()
    {
        float currentGearRation = this._currentGear >= 0 ? this.Gearbox.ForwardGearRatios[this._currentGear] : this.Gearbox.ReverseRatio;
        float maxWheelRpm = this._currentEngineRpm / (currentGearRation * this.Gearbox.FinalDriveRatio);
        return maxWheelRpm;
    }

    private void ApplyBrakeTorqueToWheels()
    {
        foreach (AxleInfo axle in this._axleInfos)
        {
            foreach (WheelCollider wheel in axle.GetAxleWheels())
            {
                wheel.brakeTorque = this._maxBrakingTorque * axle.BrakeBias * this._input.Brake;
            }
        }
    }

    private void ApplySteeringToWheels()
    {
        foreach (AxleInfo axle in this._axleInfos.Where(x => x.Steering))
        {
            foreach (WheelCollider wheel in axle.GetAxleWheels())
            {
                wheel.steerAngle = Mathf.Lerp(axle.LeftWheel.steerAngle, this._input.Steering * this._maxSteeringAngle, this._steerSpeed);
            }
        }
    }

    private void ApplyHandbrake()
    {
        if (this._input.Handbrake)
        {
            foreach (WheelCollider wheel in this._axleInfos.Where(x => x.HandBrake).FirstOrDefault().GetAxleWheels())
            {
                wheel.motorTorque = 0.0001f;
                wheel.brakeTorque = this._handbrakeTorque;
            }
        }
    }

    private void RevEngine()
    {
        float currentThrottle = this._input.Throttle;
        if (this._currentEngineRpm >= this.Engine.MaximumRmp || this._cutThrottle)
        {
            currentThrottle = 0f;
        }

        float currentGearRatio = this._currentGear >= 0 ? this.Gearbox.ForwardGearRatios[this._currentGear] : this.Gearbox.ReverseRatio;
        float effInertia = this.Engine.Inertia + this._input.Clutch * (this._wheelInertia * Mathf.Abs(currentGearRatio));
        this._currentBackdriveTorque = 0f;

        if (this._currentGear != 0 && this._input.Clutch == 1)
        {
            float wheelRPM = 0f;
            float newRpm = 0f;
            WheelHit hit;

            foreach (AxleInfo axle in this._axleInfos.Where(x => x.Motor))
            {
                float totalWheelRpm = 0f;

                foreach (WheelCollider wheel in axle.GetAxleWheels())
                {
                    if (wheel.GetGroundHit(out hit))
                    {
                        wheelRPM = GetWheelGroundRPM(wheel) * currentGearRatio * this.Gearbox.FinalDriveRatio;
                        newRpm = this._input.Clutch * this._currentEngineRpm + (1 - this._input.Clutch) * wheelRPM;
                        var wheelTorque = (this._currentEngineRpm - newRpm) * this._wheelInertia;
                        this._currentBackdriveTorque += wheelTorque;
                    }
                    totalWheelRpm += wheel.rpm;
                }

                var newWheelRpm = totalWheelRpm / axle.GetAxleWheels().Length;

                float calculatedRpm = newWheelRpm * currentGearRatio * this.Gearbox.FinalDriveRatio;
                this._currentEngineRpm = Mathf.Clamp(calculatedRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);
            }
        }

        this._currentBackdriveTorque = Mathf.Clamp(this._currentBackdriveTorque, -1e8f, 1e8f);

        this._currentEngineRpm = Mathf.Clamp(this._currentEngineRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);

        float momentum = this._currentEngineRpm * effInertia;
        momentum += this.GetEngineTorque() * currentThrottle;
        momentum -= this._currentBackdriveTorque;
        momentum -= this.Engine.Friction * this._currentEngineRpm;
        
        this._currentEngineRpm = momentum / effInertia;
        this._currentEngineRpm = Mathf.Clamp(this._currentEngineRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp); 
        this._currentEngineTorque = GetEngineTorque() * currentThrottle;

        if (this._isHybridBoostApplied && this._isHybridBoostAvailable)
        {
            this._currentEngineTorque += this._hybridBoostTorque;
        }

        this._currentTransmissionTorque =
            this._currentEngineTorque *
            currentGearRatio *
            this.Gearbox.FinalDriveRatio * 
            this.Gearbox.KPD *
            this._input.Clutch;
    }

    private void ApplyHybridBoost()
    {
        if (this._enableHybridBoost)
        {
            if (this._input.Boost && this._isHybridBoostAvailable && this._currentSpeed > this._speedThreshold)
            {
                this._isHybridBoostApplied = true;
                this._currentBoostAmount -= Time.deltaTime * _hybridBoostDepletionRate;
                if (this._currentBoostAmount <= 0)
                {
                    this._currentBoostAmount = 0;
                    this._isHybridBoostAvailable = false;
                }
            }
            else
            {
                this._isHybridBoostApplied = false;
            }
        }
    }

    private void RechargeHybridBoost()
    {
        if (this._input.Brake > 0f)
        {
            List<WheelCollider> wheels = new List<WheelCollider>();
            foreach (AxleInfo axle in this._axleInfos)
            {
                foreach (WheelCollider wheel in axle.GetAxleWheels())
                {
                    wheels.Add(wheel);
                }
            }

            if (wheels.Any(x => x.rpm > 1f) && this._currentSpeed > this._speedThreshold)
            {
                float boostToAdd = Time.deltaTime * this._hybridBoostRechargeRate * this._input.Brake;
                this._currentBoostAmount = Mathf.Min(this._currentBoostAmount + boostToAdd, _maxBoostAmount);
                if (this._currentBoostAmount >= this._hybridBoostRequiredAmount)
                {
                    this._isHybridBoostAvailable = true;
                }
            }         
        }
    }

    public float GetWheelGroundRPM(WheelCollider wheel)
    {
        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            float SlipInDirection = hit.forwardSlip / (radiansToRevs * wheel.radius) / radiansToRevs * 60 / Time.deltaTime;
            return wheel.rpm - SlipInDirection;
        }
        else return wheel.rpm;
    }

    private float GetEngineTorque()
    {
        int roundedRpms = (int) Mathf.Floor(this._currentEngineRpm / 100f) * 100;
        return this.Engine.TorqueCurve[roundedRpms];
    }

    private void AddDownforce()
    {
        this._myRigidBody.AddForce(-transform.up * this._downforce * this._currentSpeed);
    }

    private void GetCurrentSpeed()
    {
        this._currentSpeed = this._myRigidBody.velocity.magnitude * 3.6f;
    }

    private void DetectWheelSlip()
    {
        foreach (AxleInfo axle in this._axleInfos.Where(x => x.Motor))
        {
            WheelHit hitLeftWheel;
            WheelHit hitRightWheel;

            if (axle.LeftWheel.GetGroundHit(out hitLeftWheel))
            {
                if (hitLeftWheel.forwardSlip < 0)
                {
                    // Debug.Log("acceleration slip left!");
                }
            }

            if (axle.RightWheel.GetGroundHit(out hitRightWheel))
            {
                if (hitLeftWheel.forwardSlip < 0)
                {
                    // Debug.Log("acceleration slip right!");
                }
            }
        }
    }

    private void AdjustWheelFriction()
    {
        foreach (AxleInfo axle in this._axleInfos)
        {
            foreach (WheelCollider wheel in axle.GetAxleWheels())
            {
                WheelHit hit;
                if (wheel.GetGroundHit(out hit))
                {
                    WheelFrictionCurve sidewaysFrinction = wheel.sidewaysFriction;
                    if (hit.collider.gameObject.CompareTag("OffTrackSurface"))
                    {
                        sidewaysFrinction.stiffness = _offTrackFriction;
                    }
                    else if (wheel.rpm <= 0)
                    {
                        sidewaysFrinction.stiffness =
                            this._input.Handbrake ?
                            _lockedHandBrakeSidewaysFriction :
                            _lockedBrakesSidewaysFriction;
                    }
                    else
                    {
                        sidewaysFrinction.stiffness = _defaultSidewaysFriction;
                    }
                    wheel.sidewaysFriction = sidewaysFrinction;
                }
            }
        }
    }
}