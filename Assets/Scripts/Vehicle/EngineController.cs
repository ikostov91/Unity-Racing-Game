using Assets.Scripts;
using Assets.Scripts.PlayerInput;
using UnityEngine;

[RequireComponent(typeof(IInput))]
public class EngineController : MonoBehaviour
{
    private IInput _input;
    private IEngine Engine;
    private IGearbox Gearbox;

    private float _currentEngineRpm;
    private float _currentEngineTorque = 0f;
    private float _currentBackdriveTorque = 0f;
    private float _wheelInertia = 0.92f;

    public float EngineTorque => this._currentEngineTorque;

    void Start()
    {
        this._input = GetComponent<IInput>();
        this.Engine = GetComponent<IEngine>();
        this.Gearbox = GetComponent<IGearbox>();

        this._currentEngineRpm = this.Engine.MinimumRpm;
    }

    // Update is called once per frame
    void Update()
    {
        this.RevEngine();
    }

    private void RevEngine()
    {
        //float currentThrottle = this._input.Throttle;
        //if (this._currentEngineRpm >= this.Engine.MaximumRmp || cutThrottle)
        //{
        //    currentThrottle = 0f;
        //}

        //float effInertia = this.Engine.Inertia + this._input.Clutch * (this._wheelInertia * Mathf.Abs(gearRatio));
        //this._currentBackdriveTorque = 0f;

        //if (gear != 0 && this._input.Clutch == 1)
        //{
        //    float wheelRPM = 0f;
        //    float newRpm = 0f;
        //    WheelHit hit;

        //    foreach (AxleInfo axle in motorAxels)
        //    {
        //        float totalWheelRpm = 0f;

        //        foreach (WheelCollider wheel in axle.GetAxleWheels())
        //        {
        //            if (wheel.GetGroundHit(out hit))
        //            {
        //                wheelRPM = GetWheelGroundRPM(wheel) * gearRatio * this.Gearbox.FinalDriveRatio;
        //                newRpm = this._input.Clutch * this._currentEngineRpm + (1 - this._input.Clutch) * wheelRPM;
        //                var wheelTorque = (this._currentEngineRpm - newRpm) * this._wheelInertia;
        //                this._currentBackdriveTorque += wheelTorque;
        //            }
        //            totalWheelRpm += wheel.rpm;
        //        }

        //        var newWheelRpm = totalWheelRpm / axle.GetAxleWheels().Length;

        //        float calculatedRpm = newWheelRpm * gearRatio * this.Gearbox.FinalDriveRatio;
        //        this._currentEngineRpm = Mathf.Clamp(calculatedRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);
        //    }
        //}

        //this._currentBackdriveTorque = Mathf.Clamp(this._currentBackdriveTorque, -1e8f, 1e8f);

        //this._currentEngineRpm = Mathf.Clamp(this._currentEngineRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);

        //float momentum = this._currentEngineRpm * effInertia;
        //momentum += this.GetEngineTorque() * currentThrottle;
        //momentum -= this._currentBackdriveTorque;
        //momentum -= this.Engine.Friction * this._currentEngineRpm;

        //this._currentEngineRpm = momentum / effInertia;
        //this._currentEngineRpm = Mathf.Clamp(this._currentEngineRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);
        //this._currentEngineTorque = GetEngineTorque() * currentThrottle;
    }

    private float GetEngineTorque()
    {
        int roundedRpms = (int)Mathf.Floor(this._currentEngineRpm / 100f) * 100;
        return this.Engine.TorqueCurve[roundedRpms];
    }
}
