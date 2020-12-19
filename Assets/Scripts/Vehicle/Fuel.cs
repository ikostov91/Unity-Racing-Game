using Assets.Scripts.PlayerInput;
using UnityEngine;

public class Fuel : MonoBehaviour
{
    [Range(1f, 200f)]
    [SerializeField] private float _maxFuelAmount = 50f;
    [SerializeField] private float _currentFuelAmount = 50f;

    [SerializeField] private float _fuelConsumptionRate = 0.15f;

    private VehicleController _vehicleController;
    private IInput _input;

    public float CurrentFuel => this._currentFuelAmount;

    void Start()
    {
        this._vehicleController = GetComponent<VehicleController>();
        this._input = GetComponent<IInput>();
    }

    void Update()
    {
        this.ConsumeFuel();
        this.Refuel();
    }

    private void ConsumeFuel()
    {
        float throttleInput = this._input.Throttle;
        float currentSpeed = this._vehicleController.Speed;
        float currentGear = this._vehicleController.CurrentGear;

        float consumedFuel = this._fuelConsumptionRate * Time.deltaTime * Mathf.Max(0.1f, throttleInput);
        this._currentFuelAmount = Mathf.Max(this._currentFuelAmount -= consumedFuel, 0f);

        if (this._currentFuelAmount == 0f)
        {
            this._vehicleController.CutTrottle = true;
        }
    }

    private void Refuel()
    {
        // TODO refuel logic
        if (Input.GetKeyDown(KeyCode.G))
        {
            this._currentFuelAmount = Mathf.Min(this._maxFuelAmount, this._currentFuelAmount + 50f);
            this._vehicleController.CutTrottle = false;
        }
    }
}
