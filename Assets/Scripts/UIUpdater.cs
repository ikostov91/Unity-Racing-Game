using UnityEngine;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour
{
    private VehicleController _vehicleController;

    [SerializeField] private Text _engineRevsDisplay;
    [SerializeField] private Slider _revCounterDisplay;
    [SerializeField] private Text _gearDisplay;
    [SerializeField] private Text _gearboxModeDisplay;
    [SerializeField] private Text _torqueOutput;
    [SerializeField] private Slider _boostAmount;
    [SerializeField] private Image _boostSliderFill;
    [SerializeField] private Text _boostLabel;

    void Start()
    {
        this._vehicleController = FindObjectOfType<VehicleController>();

        this.SetHybridBoostGaugeState();
    }

    void Update()
    {
        this.UpdateRevCounter();
        this.UpdateRevsDisplay();
        this.UpdateCurrentGear();
        this.UpdateGearboxMode();
        this.UpdateTorqueOutput();
        this.UpdateHybridBoostGauge();
    }

    private void SetHybridBoostGaugeState()
    {
        bool boostEnabled = this._vehicleController.HybridBoostEnabled;

        this._boostAmount.gameObject.SetActive(boostEnabled);
        this._boostSliderFill.gameObject.SetActive(boostEnabled);
        this._boostLabel.gameObject.SetActive(boostEnabled);
    }

    private void UpdateRevCounter()
    {
        var currentRevCounter = Mathf.InverseLerp(
                this._vehicleController.Engine.MinimumRpm,
                this._vehicleController.Engine.MaximumRmp,
                this._vehicleController.EngineRpm
            );
        this._revCounterDisplay.value = currentRevCounter;
    }

    private void UpdateRevsDisplay()
    {
        this._engineRevsDisplay.text = $"Revs: {Mathf.RoundToInt(this._vehicleController.EngineRpm)}";

        if (this._vehicleController.EngineRpm > this._vehicleController.Engine.RedLine)
        {
            this._engineRevsDisplay.color = Color.red;
        }
        else
        {
            this._engineRevsDisplay.color = Color.yellow;
        }
    }

    private void UpdateCurrentGear()
    {
        string GetGearText(int currentGear)
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

        this._gearDisplay.text = $"Gear: {GetGearText(this._vehicleController.CurrentGear)}";
    }

    private void UpdateGearboxMode()
    {
        this._gearboxModeDisplay.text = this._vehicleController.AutomaticGearbox ? Constants.GearboxConstants.GearboxAutoMode : Constants.GearboxConstants.GearboxManualMode;
    }

    private void UpdateTorqueOutput()
    {
        this._torqueOutput.text = $"Torque Output: {Mathf.RoundToInt(this._vehicleController.EngineTorque)}";
    }

    private void UpdateHybridBoostGauge()
    {
        var currentBoostAmount = Mathf.InverseLerp(0f, 100f, this._vehicleController.BoostAmount);
        this._boostAmount.value = currentBoostAmount;
        if (this._vehicleController.HybridBoostAvailable)
        {
            this._boostSliderFill.color = Color.red;
        }
        else
        {
            this._boostSliderFill.color = Color.grey;
        }
    }
}
