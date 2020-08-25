using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VehicleController : MonoBehaviour
{
    [SerializeField] private float _maxSteeringAngle = 35f;
    [SerializeField] private float _maxBrakingTorque = 4500f;
    [SerializeField] private float _handbrakeTorque = 10000f;

    private float _currentEngineRpm;
    private int _currentGear = 0;

    private int _throttleInput = 0;
    private int _brakeInput = 0;
    private int _clutchInput = 1;
    private int _handBrakeInput = 0;
    private float _steerInput = 0;

    private bool _cutThrottle = false;

    private Rigidbody _myRigidBody;

    [SerializeField] public AxleInfo[] _axleInfos;

    [SerializeField] private Engine Engine;
    [SerializeField] private Gearbox Gearbox;

    [SerializeField] private Text _engineRevsDisplay;
    [SerializeField] private Slider _revCounterDisplay;
    [SerializeField] private Text _gearDisplay;
    [SerializeField] private Text _gearboxModeDisplay;

    [SerializeField] private Transform _centerOfMass;

    [Range(0.5f, 600f)]
    [SerializeField] private float _downforce = 1.0f;

    private float _currentEngineTorque = 0f;
    private float _currentTransmissionTorque = 0.0001f;
    private float _currentBackdriveTorque = 0f;
    private float _wheelInertia = 0.92f;

    [Range(0.001f, 1.0f)]
    [SerializeField] private float _steerSpeed = 0.2f;

    [SerializeField] private float _shiftUpTime = 1.0f;
    [SerializeField] private float _shiftDownTime = 0.2f;

    [SerializeField] private float _speed = 0.0f;

    private const float radiansToRevs = 0.159155f;

    [SerializeField] AnimationCurve turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

    [SerializeField] float _hybridBoostPower = 160f;
    private bool _isHybridBoostApplied = false;

    [SerializeField] private bool _isGearboxAutomatic = false;

    // 1e8f == 100000000f

    void Start()
    {
        this._currentEngineRpm = this.Engine.MinimumRpm;

        this._myRigidBody = GetComponent<Rigidbody>();

        if (this._myRigidBody != null && this._centerOfMass != null)
        {
            this._myRigidBody.centerOfMass = this._centerOfMass.localPosition;
        }

        foreach (var axle in this._axleInfos)
        {
            if (axle.Motor)
            {
                axle.LeftWheel.motorTorque = this._currentTransmissionTorque;
                axle.RightWheel.motorTorque = this._currentTransmissionTorque;
            }
        }
    }

    void FixedUpdate()
    {
        this.ApplyTransmissionTorqueToWheels();
        this.ApplyBrakeTorqueToWheels();
        this.ApplySteeringToWheels();
        this.ApplyHandbrake();
        this.AddDownforce();
        this.LockWheelsRotation();

        this.ApplyHybridBoost();
    }

    void Update()
    {
        this.ThrottleInput();
        this.BrakesInput();
        this.SteeringInput();
        this.ChangeGears();
        this.ClutchInput();
        this.HandbrakeInput();
        this.AnimateWheels();
        this.DetectWheelSlip();
        this.RevEngine();
        this.GetCurrentSpeed();

        this.UpdateUI();
        this.SwitchGearboxMode();
    }

    private void ThrottleInput()
    {
        if (Input.GetKey(KeyCode.UpArrow) && !this._cutThrottle)
        {
            this._throttleInput = 1;
        }
        else
        {
            this._throttleInput = 0;
        }
    }

    private void BrakesInput()
    {
        if (Input.GetKey(KeyCode.DownArrow))
        {
            this._brakeInput = 1;
        }
        else
        {
            this._brakeInput = 0;
        }
    }

    private void SteeringInput()
    {
        this._steerInput = turnInputCurve.Evaluate(Input.GetAxis("Horizontal")) * this._maxSteeringAngle;
    }

    private void ClutchInput()
    {
        if (Input.GetKey(KeyCode.C))
        {
            this._clutchInput = 0;
        }
        else
        {
            this._clutchInput = 1;
        }
    }

    private void HandbrakeInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            this._handBrakeInput = 1;
        }
        else
        {
            this._handBrakeInput = 0;
        }
    }

    private void ChangeGears()
    {
        if (!this._isGearboxAutomatic)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                int nextGear = Mathf.Min(this._currentGear + 1, this.Gearbox.HighestGear);
                if (!this.CheckIfGearChangeIsPossible(nextGear))
                {
                    return;
                }

                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftUpTime));
            }
            else if (Input.GetKeyDown(KeyCode.X))
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
                this._speed > 1f &&
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
                this._speed < 1f &&
                this._currentGear == this.Gearbox.NeutralGear)
            {
                int nextGear = this.Gearbox.FirstGear;
                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftDownTime));
            }
            else if (Input.GetKeyDown(KeyCode.S) &&
                this._speed <= 1f &&
                (this._currentGear < this.Gearbox.FirstGear))
            {
                int nextGear = this.Gearbox.FirstGear;
                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftUpTime));
            }
            else if (Input.GetKeyDown(KeyCode.X) && this._speed <= 1f)
            {
                int nextGear = this.Gearbox.ReverseGear;
                this.StartCoroutine(this.ShiftIntoNewGear(nextGear, this._shiftDownTime));
            }
        }
    }

    private bool CheckIfGearChangeIsPossible(int nextGear)
    {
        if (this._currentGear > 0 && nextGear >= 1 && nextGear <= this.Gearbox.HighestGear && this._speed > 10f)
        {
            AxleInfo motorAxle = this._axleInfos.FirstOrDefault(x => x.Motor);
            float wheelRpm = (motorAxle.RightWheel.rpm + motorAxle.LeftWheel.rpm) / 2;
            float engineRpm = wheelRpm * this.Gearbox.ForwardGearRatios[nextGear] * this.Gearbox.FinalDriveRatio;
            if (engineRpm < this.Engine.MinimumRpm || engineRpm > this.Engine.MaximumRmp)
            {
                return false;
            }
        }
        else if (this._currentGear == -1 && nextGear >= 0 && this._speed > 10f)
        {
            return false;
        }
        else if (this._currentGear == 0 && nextGear == -1 && this._speed > 10f)
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

        foreach (var axle in this._axleInfos)
        {
            if (axle.Motor)
            {
                if (this._currentGear != 0)
                {
                    float currentGearRation = this._currentGear >= 0 ? this.Gearbox.ForwardGearRatios[this._currentGear] : this.Gearbox.ReverseRatio;
                    thrustTorque = this._currentTransmissionTorque;
                    backdriveTorque = Mathf.Clamp(this._currentBackdriveTorque, 0, 1e8f) / currentGearRation;
                    engineBrakeTorque = Mathf.Clamp(this._currentBackdriveTorque, -1e8f, 0f) / currentGearRation;
                }

                float totalTorque = thrustTorque + backdriveTorque;
                if (this._isHybridBoostApplied)
                {
                    totalTorque += this._hybridBoostPower;
                }
                float maxWheelRpm = this.GetMaximumWheelRpmPossible();

                if (axle.LeftWheel.rpm >= maxWheelRpm || axle.RightWheel.rpm >= maxWheelRpm)
                {
                    axle.LeftWheel.motorTorque = 0.0001f;
                    axle.RightWheel.motorTorque = 0.0001f;
                }
                else
                {
                    int torqueDirection = this._currentGear >= 0 ? 1 : -1;
                    axle.LeftWheel.motorTorque = totalTorque * torqueDirection;
                    axle.RightWheel.motorTorque = totalTorque * torqueDirection;
                }

                axle.LeftWheel.brakeTorque += Mathf.Abs(engineBrakeTorque);
                axle.RightWheel.brakeTorque += Mathf.Abs(engineBrakeTorque);
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
        foreach (var axle in this._axleInfos)
        {
            axle.LeftWheel.brakeTorque = this._maxBrakingTorque * axle.BrakeBias * this._brakeInput;
            axle.RightWheel.brakeTorque = this._maxBrakingTorque * axle.BrakeBias * this._brakeInput;
        }
    }

    private void ApplySteeringToWheels()
    {
        foreach (var axle in this._axleInfos)
        {
            if (axle.Steering)
            {
                axle.LeftWheel.steerAngle = Mathf.Lerp(axle.LeftWheel.steerAngle, this._steerInput, this._steerSpeed);
                axle.RightWheel.steerAngle = Mathf.Lerp(axle.LeftWheel.steerAngle, this._steerInput, this._steerSpeed);
            }
        }
    }

    private void ApplyHandbrake()
    {
        if (this._handBrakeInput == 1)
        {
            this._axleInfos.Last().LeftWheel.motorTorque = 0.0001f;
            this._axleInfos.Last().LeftWheel.brakeTorque = this._handbrakeTorque;

            this._axleInfos.Last().RightWheel.motorTorque = 0.0001f;
            this._axleInfos.Last().RightWheel.brakeTorque = this._handbrakeTorque;
        }
    }

    private void RevEngine()
    {
        if (this._currentEngineRpm >= this.Engine.MaximumRmp)
        {
            this._throttleInput = 0;
        }

        float currentGearRatio = this._currentGear >= 0 ? this.Gearbox.ForwardGearRatios[this._currentGear] : this.Gearbox.ReverseRatio;
        float effInertia = this.Engine.Inertia + this._clutchInput * (this._wheelInertia * Mathf.Abs(currentGearRatio));
        this._currentBackdriveTorque = 0f;

        if (this._currentGear != 0 && this._clutchInput == 1)
        {
            float wheelRPM = 0f;
            float newRpm = 0f;
            WheelHit leftHit, rightHit;

            foreach (var axle in this._axleInfos)
            {
                if (axle.Motor)
                {
                    if (axle.LeftWheel.GetGroundHit(out leftHit))
                    {
                        wheelRPM = GetWheelGroundRPM(axle.LeftWheel) * currentGearRatio * this.Gearbox.FinalDriveRatio;
                        newRpm = this._clutchInput * this._currentEngineRpm + (1 - this._clutchInput) * wheelRPM;
                        var wheelTorque = ((this._currentEngineRpm - newRpm) * this._wheelInertia);
                        this._currentBackdriveTorque += wheelTorque;
                    }

                    if (axle.RightWheel.GetGroundHit(out rightHit))
                    {
                        wheelRPM = GetWheelGroundRPM(axle.RightWheel) * currentGearRatio * this.Gearbox.FinalDriveRatio;
                        newRpm = this._clutchInput * this._currentEngineRpm + (1 - this._clutchInput) * wheelRPM;
                        var wheelTorque = ((this._currentEngineRpm - newRpm) * this._wheelInertia);
                        this._currentBackdriveTorque += wheelTorque;
                    }

                    var newWheelRpm = (axle.LeftWheel.rpm + axle.RightWheel.rpm) / 2;

                    float calculatedRpm = newWheelRpm * currentGearRatio * this.Gearbox.FinalDriveRatio;
                    this._currentEngineRpm = Mathf.Clamp(calculatedRpm, this.Engine.MinimumRpm, this.Engine.MaximumRmp);
                }
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
        this._currentTransmissionTorque =
            this._currentEngineTorque *
            currentGearRatio *
            this.Gearbox.FinalDriveRatio * 
            this.Gearbox.KPD *
            this._clutchInput;
    }
    
    private void LockWheelsRotation()
    {
        foreach (var axle in this._axleInfos)
        {
            if (axle.Motor)
            {
                float leftWheelRpm = axle.LeftWheel.rpm;
                float rightWheelRpm = axle.RightWheel.rpm;

                if (leftWheelRpm < rightWheelRpm)
                {
                    // axle.LeftWheel.motorTorque += _torqueToWheel;
                }
                else if (rightWheelRpm < leftWheelRpm)
                {
                    // axle.RightWheel.motorTorque += _torqueToWheel;
                }
            }
        }
    }

    private void ApplyHybridBoost()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            this._isHybridBoostApplied = true;
        }
        else
        {
            this._isHybridBoostApplied = false;
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
        this._myRigidBody.AddForce(-transform.up * this._downforce * this._speed);
    }

    private void GetCurrentSpeed()
    {
        this._speed = this._myRigidBody.velocity.magnitude * 3.6f;
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

    private void UpdateUI()
    {
        var currentRevCounter = Mathf.InverseLerp(Engine.MinimumRpm, Engine.MaximumRmp, this._currentEngineRpm);
        this._revCounterDisplay.value = currentRevCounter;

        this._engineRevsDisplay.text = $"Revs: {Mathf.RoundToInt(this._currentEngineRpm)}";

        if (this._currentEngineRpm > this.Engine.RedLine)
        {
            this._engineRevsDisplay.color = Color.red;
        }
        else
        {
            this._engineRevsDisplay.color = Color.yellow;
        }

        string UpdateGearDisplay(int currentGear)
        {
            if (currentGear == 0)
            {
                return Constants.GearboxConstants.GearboxNeutralGear;
            }
            
            if (currentGear == -1)
            {
                return Constants.GearboxConstants.GearboxReverseGear;
            }

            return currentGear.ToString();
        }

        this._gearDisplay.text = $"Gear: {UpdateGearDisplay(this._currentGear)}";

        this._gearboxModeDisplay.text = this._isGearboxAutomatic ? Constants.GearboxConstants.GearboxAutoMode : Constants.GearboxConstants.GearboxManualMode;
    }

    private void SwitchGearboxMode()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            this._isGearboxAutomatic = !this._isGearboxAutomatic;
        }
    }

    private void DetectWheelSlip()
    {
        foreach (var axle in this._axleInfos)
        {
            if (axle.Motor)
            {
                WheelHit hitLeftWheel;
                WheelHit hitRightWheel;

                if (axle.LeftWheel.GetGroundHit(out hitLeftWheel))
                {
                    // Debug.Log(hitLeftWheel.forwardSlip);
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
    }
}