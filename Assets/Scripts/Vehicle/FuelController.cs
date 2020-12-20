using Assets.Scripts.PlayerInput;
using UnityEngine;

public class FuelController : MonoBehaviour
{
    [Range(1f, 200f)]
    [SerializeField] private float _maxFuelAmount = 50f;
    [SerializeField] private float _fuelAmount = 50f;

    [SerializeField] private float _fuelConsumptionRate = 0.15f;

    private IInput _input;

    public float FuelAmount => this._fuelAmount;
    public bool HasFuel => this._fuelAmount > 0f;

    void Start()
    {
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

        float consumedFuel = this._fuelConsumptionRate * Time.deltaTime * Mathf.Max(0.1f, throttleInput);
        this._fuelAmount = Mathf.Max(this._fuelAmount -= consumedFuel, 0f);
    }

    private void Refuel()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            this._fuelAmount = Mathf.Min(this._maxFuelAmount, this._fuelAmount + 50f);
        }
    }
}
