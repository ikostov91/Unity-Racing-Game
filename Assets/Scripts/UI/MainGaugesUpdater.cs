using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainGaugesUpdater : MonoBehaviour
{
    private VehicleController _vehicleController;
    private EngineController _engineController;
    private GearboxController _gearboxController;
    private FuelController _fuelController;

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
        this._engineController = FindObjectOfType<EngineController>();
        this._gearboxController = FindObjectOfType<GearboxController>();
        this._fuelController = this._vehicleController.GetComponent<FuelController>();

        this.SetBoostGaugeState();
    }

    void Update()
    {
        this.UpdateSpeedDisplay();
        this.UpdateRevCounter();
        this.UpdateRevsDisplay();
        this.UpdateCurrentGear();
        this.UpdateBoostGauge();
        this.UpdateFuelDisplay();
    }

    private void UpdateSpeedDisplay()
    {
        this._speedDisplay.text = Mathf.RoundToInt(this._vehicleController.Speed).ToString("D3");
    }

    private void SetBoostGaugeState()
    {
        bool boostEnabled = this._vehicleController.TryGetComponent<BoostController>(out _);

        this._boostAmount.gameObject.SetActive(boostEnabled);
        this._boostSliderFill.gameObject.SetActive(boostEnabled);
        this._boostLabel.gameObject.SetActive(boostEnabled);
    }

    private void UpdateRevCounter()
    {
        var currentRevCounter = Mathf.InverseLerp(
                this._engineController.EngineMinRpm,
                this._engineController.EngineMaxRpm,
                this._engineController.EngineRpm
            );
        this._revCounterDisplay.value = currentRevCounter;

        Image fill = this._revCounterDisplay.GetComponentsInChildren<Image>()
            .FirstOrDefault(t => t.name == "Fill");
        if (fill != null)
        {
            if (this._engineController.EngineRpm > this._engineController.EngineRedLine)
            {
                fill.color = Color.red;
            }
            else
            {
                fill.color = Color.yellow;
            }
        }
    }

    private void UpdateRevsDisplay()
    {
        this._engineRevsDisplay.text = Mathf.RoundToInt(this._engineController.EngineRpm).ToString();

        if (this._engineController.EngineRpm > this._engineController.EngineRedLine)
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
            switch (currentGear)
            {
                case 0:
                    return Constants.GearboxConstants.GearboxNeutralGear;
                case -1:
                    return Constants.GearboxConstants.GearboxReverseGear;
                default:
                    return currentGear.ToString();
            }
        }

        this._gearDisplay.text = $"{GetGearText(this._gearboxController.Gear)}";
    }

    private void UpdateBoostGauge()
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
        this._fuelDisplay.text = this._fuelController.FuelAmount.ToString("F1");
    }
}
