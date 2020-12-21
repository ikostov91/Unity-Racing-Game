using Assets.Scripts;
using Assets.Scripts.PlayerInput;
using UnityEngine;

[RequireComponent(typeof(IInput))]
public class EngineController : MonoBehaviour
{
    private VehicleController _vehicleController;
    private GearboxController _gearboxController;
    private FuelController _fuelController;
    private IInput _input;
    private IEngine Engine;
    private IGearbox Gearbox;

    private float _engineRpm;
    private float _engineTorque = 0f;
    private float _backdriveTorque = 0f;
    private const float _wheelInertia = 0.92f;
    private const float radiansToRevs = 0.159155f;

    private bool _throttleCut = false;

    public float EngineRpm => this._engineRpm;
    public float BackdriveTorque => this._backdriveTorque;
    public float EngineTorque
    {
        get => this._engineTorque;
        set => this._engineTorque = value;
    }
    public bool ThrottleCut
    {
        get => this._throttleCut;
        set => this._throttleCut = value;
    }

    void Start()
    {
        this._vehicleController = GetComponent<VehicleController>();
        this._gearboxController = GetComponent<GearboxController>();
        this._fuelController = GetComponent<FuelController>();

        this._input = GetComponent<IInput>();
        this.Engine = GetComponent<IEngine>();
        this.Gearbox = GetComponent<IGearbox>();

        this._engineRpm = this.Engine.MinimumRpm;
    }

    void Update()
    {
        this.GetEngineTorque();
    }

    private void GetEngineTorque()
    {
        float currentThrottle = this._input.Throttle;
        if (this._engineRpm >= this.Engine.MaximumRmp || this._throttleCut || !this._fuelController.HasFuel)
        {
            currentThrottle = 0f;
        }

        float currentGearRatio = this.Gearbox.GearRatios[this._gearboxController.Gear];
        float effInertia = this.Engine.Inertia + this._input.Clutch * (_wheelInertia * Mathf.Abs(currentGearRatio));
        this._backdriveTorque = 0f;

        if (this._gearboxController.Gear != 0 && this._input.Clutch != 0f)
        {
            float wheelRPM = 0f;
            float newRpm = 0f;
            WheelHit hit;

            foreach (AxleInfo axle in this._vehicleController.MotorAxles)
            {
                float totalWheelRpm = 0f;

                foreach (WheelCollider wheel in axle.GetAxleWheels())
                {
                    if (wheel.GetGroundHit(out hit))
                    {
                        wheelRPM = GetWheelGroundRPM(wheel) * currentGearRatio * this.Gearbox.FinalDriveRatio;
                        newRpm = this._input.Clutch * this._engineRpm + (1 - this._input.Clutch) * wheelRPM;
                        var wheelTorque = (this._engineRpm - newRpm) * _wheelInertia;
                        this._backdriveTorque += wheelTorque;
                    }
                    totalWheelRpm += wheel.rpm;
                }

                var newWheelRpm = totalWheelRpm / axle.GetAxleWheels().Length;

                float calculatedRpm = newWheelRpm * currentGearRatio * this.Gearbox.FinalDriveRatio;
                this._engineRpm = Mathf.Clamp(calculatedRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);
            }
        }

        this._backdriveTorque = Mathf.Clamp(this._backdriveTorque, -1e8f, 1e8f);

        this._engineRpm = Mathf.Clamp(this._engineRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);

        float momentum = this._engineRpm * effInertia;
        momentum += this.GetTorqueByRpm() * currentThrottle;
        momentum -= this._backdriveTorque;
        momentum -= this.Engine.Friction * this._engineRpm;

        this._engineRpm = momentum / effInertia;
        this._engineRpm = Mathf.Clamp(this._engineRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);
        this._engineTorque = GetTorqueByRpm() * currentThrottle;
    }

    private float GetTorqueByRpm()
    {
        int roundedRpms = (int)Mathf.Floor(this._engineRpm / 100f) * 100;
        return this.Engine.TorqueCurve[roundedRpms];
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
}
