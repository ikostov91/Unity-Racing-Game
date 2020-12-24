using Assets.Scripts;
using System.Linq;
using UnityEngine;
using Assets.Scripts.PlayerInput;

[RequireComponent(typeof(IInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EngineController))]
[RequireComponent(typeof(GearboxController))]
[RequireComponent(typeof(DownforceController))]
public class VehicleController : MonoBehaviour
{
    [Header("Adjustable Parameters")]
    [SerializeField] private float _maxSteeringAngle = 35f;
    [SerializeField] private float _maxBrakingTorque = 4500f;
    [SerializeField] private float _handbrakeTorque = 10000f;
    [SerializeField] [Range(0.001f, 1.0f)] private float _steerSpeed = 0.2f;
    [SerializeField] private float _speedThreshold = 10f;

    private IInput _input;

    private Rigidbody _rigidBody;

    private EngineController _engineController;
    private GearboxController _gearboxController;

    [SerializeField] public AxleInfo[] _axleInfos;

    public IGearbox Gearbox;

    [SerializeField] private Transform _centerOfMass;

    private float _wheelsTorque = 0.0001f;
    private float _speed = 0.0f;

    // 1e8f == 100000000

    // Constant parameters
    [SerializeField] private float _defaultSidewaysFriction = 1.4f;
    [SerializeField] private float _defaultForwardFriction = 1.4f;
    [SerializeField] private float _lockedBrakesSidewaysFriction = 0.8f;
    [SerializeField] private float _lockedHandBrakeSidewaysFriction = 0.8f;
    [SerializeField] private float _offTrackFriction = 0.3f;

    public float Speed => this._speed;
    public float SpeedThreshold => this._speedThreshold;

    public AxleInfo[] MotorAxles => this._axleInfos.Where(x => x.Motor).ToArray();
    public AxleInfo[] AllAxles => this._axleInfos;

    void Start()
    {
        this._rigidBody = GetComponent<Rigidbody>();
        this._input = GetComponent<IInput>();

        this.Gearbox = GetComponent<IGearbox>();

        this._engineController = GetComponent<EngineController>();
        this._gearboxController = GetComponent<GearboxController>();   

        if (this._rigidBody != null && this._centerOfMass != null)
        {
            this._rigidBody.centerOfMass = this._centerOfMass.localPosition;
        }

        foreach (AxleInfo axle in this._axleInfos.Where(x => x.Motor))
        {
            axle.LeftWheel.motorTorque = this._wheelsTorque;
            axle.RightWheel.motorTorque = this._wheelsTorque;
        }
    }

    void FixedUpdate()
    {
        this.ApplyTransmissionTorqueToWheels();
        this.ApplyBrakeTorqueToWheels();
        this.ApplySteeringToWheels();
        this.ApplyHandbrake();
        this.AdjustWheelFriction();
        // this.DetectWheelSlip();
    }

    void Update()
    {
        this.GetTransmissionTorque();
        this.GetCurrentSpeed();
    }

    private void ApplyTransmissionTorqueToWheels()
    {
        float thrustTorque = 0f;
        float backdriveTorque = 0f;
        float engineBrakeTorque = 0f;

        foreach (AxleInfo axle in this._axleInfos.Where(x => x.Motor))
        {
            if (this._gearboxController.Gear != 0)
            {
                float currentGearRatio = this.Gearbox.GearRatios[this._gearboxController.Gear];
                thrustTorque = this._wheelsTorque;
                backdriveTorque = Mathf.Clamp(this._engineController.BackdriveTorque, 0, 1e8f) / currentGearRatio;
                engineBrakeTorque = Mathf.Clamp(this._engineController.BackdriveTorque, -1e8f, 0f) / currentGearRatio;
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
            else
            {
                int torqueDirection = this._gearboxController.Gear >= 0 ? 1 : -1;
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
        float currentGearRatio = this.Gearbox.GearRatios[this._gearboxController.Gear];
        float maxWheelRpm = this._engineController.EngineRpm / (currentGearRatio * this.Gearbox.FinalDriveRatio);
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

    private void GetTransmissionTorque()
    {
        float engineTorque = this._engineController.EngineTorque;
        this._wheelsTorque = this._gearboxController.GetTransmissionTorque(engineTorque);
    }

    private void GetCurrentSpeed()
    {
        this._speed = this._rigidBody.velocity.magnitude * 3.6f;
    }

    //private void DetectWheelSlip()
    //{
    //    foreach (AxleInfo axle in this._axleInfos.Where(x => x.Motor))
    //    {
    //        WheelHit hitLeftWheel;
    //        WheelHit hitRightWheel;

    //        if (axle.LeftWheel.GetGroundHit(out hitLeftWheel))
    //        {
    //            if (hitLeftWheel.forwardSlip < 0)
    //            {
    //                // Debug.Log("acceleration slip left!");
    //            }
    //        }

    //        if (axle.RightWheel.GetGroundHit(out hitRightWheel))
    //        {
    //            if (hitLeftWheel.forwardSlip < 0)
    //            {
    //                // Debug.Log("acceleration slip right!");
    //            }
    //        }
    //    }
    //}

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