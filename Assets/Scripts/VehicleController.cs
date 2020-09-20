using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [Header("Adjustable Parameters")]
    [SerializeField] private float _maxSteeringAngle = 35f;
    [SerializeField] private float _maxBrakingTorque = 4500f;
    [SerializeField] private float _handbrakeTorque = 10000f;
    [SerializeField] [Range(0.001f, 1.0f)] private float _steerSpeed = 0.2f;
    [SerializeField] private float _shiftUpTime = 1.0f;
    [SerializeField] private float _shiftDownTime = 0.2f;
    [SerializeField] [Range(0.5f, 600f)] private float _downforce = 1.0f;
    [SerializeField] private float _hybridBoostTorque = 160f;
    [SerializeField] private float _speedThreshold = 10f;

    [Header("Hybrid boost")]
    [SerializeField] private bool _enableHybridBoost = false;
    [SerializeField] private float _hybridBoostDepletionRate = 20f;
    [SerializeField] private float _hybridBoostRechargeRate = 20f;
    [Range(0f, 100f)]
    [SerializeField] private float _hybridBoostRequiredAmount = 80f;

    [Header("Gearbox mode")]
    [SerializeField] private bool _isGearboxAutomatic = false;

    private PlayerInput _playerInput;
    private Fuel _fuel;

    private float _throttleInput = 0f;
    private float _brakeInput = 0f;
    private float _steerInput = 0f;
    private bool _gearUpInput = false;
    private bool _gearDownInput = false;
    private float _clutchInput = 1f;
    private bool _handBrakeInput = false;
    private bool _hybridBoostInput = false;
    
    private bool _cutThrottle = false;

    private Rigidbody _myRigidBody;

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
    public bool AutomaticGearbox => this._isGearboxAutomatic;
    public float BoostAmount => this._currentBoostAmount;
    public bool HybridBoostAvailable => this._isHybridBoostAvailable;
    public bool HybridBoostEnabled => this._enableHybridBoost;
    public float Speed => this._currentSpeed;
    public bool CutTrottle
    {
        get => this._cutThrottle;
        set => this._cutThrottle = value;
    }

    void Start()
    {
        this._myRigidBody = GetComponent<Rigidbody>();
        this._playerInput = GetComponent<PlayerInput>();
        this._fuel = GetComponent<Fuel>();
        this.Engine = GetComponent<IEngine>();
        this.Gearbox = GetComponent<IGearbox>();

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
        this.ProcessInput();
        this.ChangeGears();
        this.AnimateWheels();
        this.RevEngine();
        this.GetCurrentSpeed();
        this.SwitchGearboxMode();
        this.ApplyHybridBoost();
        this.RechargeHybridBoost();
    }

    private void ProcessInput()
    {
        this._throttleInput = this._playerInput.Throttle;
        this._brakeInput = this._playerInput.Brake;
        this._steerInput = this._playerInput.Steering * this._maxSteeringAngle;
        this._gearUpInput = this._playerInput.GearUp;
        this._gearDownInput = this._playerInput.GearDown;
        this._clutchInput = this._playerInput.Clutch;
        this._handBrakeInput = this._playerInput.Handbrake;
        this._hybridBoostInput = this._playerInput.HybridBoost;
    }

    private void ChangeGears()
    {
        if (!this._isGearboxAutomatic)
        {
            if (this._gearUpInput)
            {
                int nextGear = Mathf.Min(this._currentGear + 1, this.Gearbox.HighestGear);
                if (!this.CheckIfGearChangeIsPossible(nextGear))
                {
                    return;
                }

                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftUpTime));
            }
            else if (this._gearDownInput)
            {
                int nextGear = Mathf.Max(this._currentGear - 1, this.Gearbox.LowestGear);
                if (!this.CheckIfGearChangeIsPossible(nextGear))
                {
                    return;
                }

                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftDownTime));
            }
        }
        else
        {
            // TODO Change input and rework
            if (Input.GetKey(KeyCode.UpArrow) && this._currentEngineRpm >= this.Engine.UpShiftRpm && this._currentGear >= this.Gearbox.FirstGear)
            {
                int nextGear = Mathf.Min(this._currentGear + 1, this.Gearbox.HighestGear);
                if (!this.CheckIfGearChangeIsPossible(nextGear))
                {
                    return;
                }

                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftUpTime));
            }
            else if (!Input.GetKey(KeyCode.UpArrow) &&
                this._currentEngineRpm < this.Engine.DownShiftRpm &&
                this._currentSpeed > 1f &&
                this._currentGear > this.Gearbox.FirstGear)
            {
                int nextGear = Mathf.Max(this._currentGear - 1, this.Gearbox.FirstGear);
                if (!this.CheckIfGearChangeIsPossible(nextGear))
                {
                    return;
                }

                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftDownTime));
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) &&
                this._currentSpeed < 1f &&
                this._currentGear == this.Gearbox.NeutralGear)
            {
                int nextGear = this.Gearbox.FirstGear;
                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftDownTime));
            }
            else if (Input.GetKeyDown(KeyCode.S) &&
                this._currentSpeed <= 1f &&
                (this._currentGear < this.Gearbox.FirstGear))
            {
                int nextGear = this.Gearbox.FirstGear;
                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftUpTime));
            }
            else if (Input.GetKeyDown(KeyCode.X) && this._currentSpeed <= 1f)
            {
                int nextGear = this.Gearbox.ReverseGear;
                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftDownTime));
            }
        }
    }

    private bool CheckIfGearChangeIsPossible(int nextGear)
    {
        if (this._currentGear > 0 && nextGear >= 1 && nextGear <= this.Gearbox.HighestGear && this._currentSpeed > this._speedThreshold)
        {
            AxleInfo motorAxle = this._axleInfos.FirstOrDefault(x => x.Motor);
            float wheelRpm = (motorAxle.RightWheel.rpm + motorAxle.LeftWheel.rpm) / 2;
            float engineRpm = wheelRpm * this.Gearbox.ForwardGearRatios[nextGear] * this.Gearbox.FinalDriveRatio;
            if (engineRpm < this.Engine.MinimumRpm || engineRpm > this.Engine.MaximumRmp)
            {
                return false;
            }
        }
        else if (this._currentGear == -1 && nextGear >= 0 && this._currentSpeed > this._speedThreshold)
        {
            return false;
        }
        else if (this._currentGear == 0 && nextGear == -1 && this._currentSpeed > this._speedThreshold)
        {
            return false;
        }

        return true;
    }

    private IEnumerator ShiftIntoNewGear(int gear, float shiftTime)
    {
        this._currentGear = 0;
        var oldThrottlePos = this._throttleInput;
        this._throttleInput = 0;
        this._cutThrottle = true;

        yield return new WaitForSeconds(shiftTime);

        this._currentGear = gear;
        this._throttleInput = oldThrottlePos;
        this._cutThrottle = false;
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
                    wheel.motorTorque = totalTorque * torqueDirection * axle.TorqueBias;
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
                wheel.brakeTorque = this._maxBrakingTorque * axle.BrakeBias * this._brakeInput;
            }
        }
    }

    private void ApplySteeringToWheels()
    {
        foreach (AxleInfo axle in this._axleInfos.Where(x => x.Steering))
        {
            foreach (WheelCollider wheel in axle.GetAxleWheels())
            {
                wheel.steerAngle = Mathf.Lerp(axle.LeftWheel.steerAngle, this._steerInput, this._steerSpeed);
            }
        }
    }

    private void ApplyHandbrake()
    {
        if (this._handBrakeInput)
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
        if (this._currentEngineRpm >= this.Engine.MaximumRmp || this._cutThrottle)
        {
            this._throttleInput = 0f;
        }

        float currentGearRatio = this._currentGear >= 0 ? this.Gearbox.ForwardGearRatios[this._currentGear] : this.Gearbox.ReverseRatio;
        float effInertia = this.Engine.Inertia + this._clutchInput * (this._wheelInertia * Mathf.Abs(currentGearRatio));
        this._currentBackdriveTorque = 0f;

        if (this._currentGear != 0 && this._clutchInput == 1)
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
                        newRpm = this._clutchInput * this._currentEngineRpm + (1 - this._clutchInput) * wheelRPM;
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
        momentum += this.GetEngineTorque() * this._throttleInput;
        momentum -= this._currentBackdriveTorque;
        momentum -= this.Engine.Friction * this._currentEngineRpm;
        
        this._currentEngineRpm = momentum / effInertia;
        this._currentEngineRpm = Mathf.Clamp(this._currentEngineRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp); 
        this._currentEngineTorque = GetEngineTorque() * this._throttleInput;

        if (this._isHybridBoostApplied && this._isHybridBoostAvailable)
        {
            this._currentEngineTorque += this._hybridBoostTorque;
        }

        this._currentTransmissionTorque =
            this._currentEngineTorque *
            currentGearRatio *
            this.Gearbox.FinalDriveRatio * 
            this.Gearbox.KPD *
            this._clutchInput;
    }

    private void ApplyHybridBoost()
    {
        if (this._enableHybridBoost)
        {
            if (this._hybridBoostInput && this._isHybridBoostAvailable && this._currentSpeed > this._speedThreshold)
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
        if (this._brakeInput > 0f)
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
                float boostToAdd = Time.deltaTime * this._hybridBoostRechargeRate * this._brakeInput;
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

    private void AnimateWheels()
    {
        foreach (AxleInfo axle in this._axleInfos)
        {
            this.ApplyLocalPositionToVisuals(wheel: axle.LeftWheel, yRotation: -180);
            this.ApplyLocalPositionToVisuals(wheel: axle.RightWheel, yRotation: 0);
        }
    }

    private void ApplyLocalPositionToVisuals(WheelCollider wheel, float yRotation)
    {
        if (wheel.transform.childCount == 0)
            return;

        Transform visualWheel = wheel.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        wheel.GetWorldPose(out position, out rotation);
        
        Quaternion newRotation = rotation * Quaternion.Euler(new Vector3(0, yRotation, 0));
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = newRotation;
    }

    private void SwitchGearboxMode()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Automatic gearbox is disabled");
            return;
            // this._isGearboxAutomatic = !this._isGearboxAutomatic;
        }
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
                            this._handBrakeInput ?
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