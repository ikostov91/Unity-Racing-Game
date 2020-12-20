using UnityEngine;
using UnityEngine.UI;

public class MainGaugesUpdater : MonoBehaviour
{
    private VehicleController _vehicleController;
    private FuelController _fuel;

    [SerializeField] private Text _engineRevsDisplay;
    [SerializeField] private Slider _revCounterDisplay;
    [SerializeField] private Text _gearDisplay;
    [SerializeField] private Text _gearboxModeDisplay;
    [SerializeField] private Slider _boostAmount;
    [SerializeField] private Image _boostSliderFill;
    [SerializeField] private Text _boostLabel;
    [SerializeField] private Text _speedDisplay;
    [SerializeField] private Text _fuelDisplay;

    void Start()
    {
        this._vehicleController = FindObjectOfType<VehicleController>();
        this._fuel = this._vehicleController.GetComponent<FuelController>();

        this.SetHybridBoostGaugeState();
    }

    void Update()
    {
        this.UpdateSpeedDisplay();
        this.UpdateRevCounter();
        this.UpdateRevsDisplay();
        this.UpdateCurrentGear();
        this.UpdateHybridBoostGauge();
        this.UpdateFuelDisplay();
    }

    private void UpdateSpeedDisplay()
    {
        this._speedDisplay.text = Mathf.RoundToInt(this._vehicleController.Speed).ToString("D3");
    }

    private void SetHybridBoostGaugeState()
    {
        bool boostEnabled = this._vehicleController.TryGetComponent<BoostController>(out _);

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
        this._engineRevsDisplay.text = Mathf.RoundToInt(this._vehicleController.EngineRpm).ToString();

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

        this._gearDisplay.text = $"{GetGearText(this._vehicleController.CurrentGear)}";
    }

    private void UpdateHybridBoostGauge()
    {
        if (this._vehicleController.TryGetComponent(out BoostController boostController))
        {
            var currentBoostAmount = Mathf.InverseLerp(0f, 100f, boostController.BoostAmount);
            this._boostAmount.value = currentBoostAmount;
            if (boostController.BoostAvailable)
            {
                this._boostSliderFill.color = Color.red;
            }
            else
            {
                this._boostSliderFill.color = Color.grey;
            }
        }
    }

    private void UpdateFuelDisplay()
    {
        this._fuelDisplay.text = this._fuel.FuelAmount.ToString("F1");
    }
}
