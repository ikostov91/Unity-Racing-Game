using Assets.Scripts.PlayerInput;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(IInput))]
public class BoostController : MonoBehaviour
{
    private IInput _input;
    private EngineController _engineController;
    private VehicleController _vehicleController;

    private float _boostAmount = 100f;
    private const float _maxBoostAmount = 100f;

    [SerializeField] private float _boostTorque = 160f;

    [Header("Hybrid boost")]
    [SerializeField] private float _depletionRate = 20f;
    [SerializeField] private float _rechargeRate = 20f;
    [Range(0f, 100f)]
    [SerializeField] private float _minimumActiveAmount = 80f;

    private bool _isBoostAvailable = true;

    public float BoostAmount => this._boostAmount;
    public bool BoostAvailable => this._isBoostAvailable;

    void Start()
    {
        this._input = GetComponent<IInput>();
        this._engineController = GetComponent<EngineController>();
        this._vehicleController = GetComponent<VehicleController>();
    }

    void Update()
    {
        this.ApplyBoost();
        this.RechargeBoost();
    }

    private void ApplyBoost()
    {
        if (this._input.Boost && this._isBoostAvailable && this._vehicleController.Speed > this._vehicleController.SpeedThreshold)
        {
            this._engineController.EngineTorque += this._boostTorque;
            this._boostAmount -= Time.deltaTime * _depletionRate;
            if (this._boostAmount <= 0)
            {
                this._boostAmount = 0;
                this._isBoostAvailable = false;
            }
        }
    }

    private void RechargeBoost()
    {
        if (this._input.Brake > 0f)
        {
            List<WheelCollider> wheels = new List<WheelCollider>();
            foreach (AxleInfo axle in this._vehicleController.AllAxles)
            {
                foreach (WheelCollider wheel in axle.GetAxleWheels())
                {
                    wheels.Add(wheel);
                }
            }

            if (wheels.Any(x => x.rpm > 1f) && this._vehicleController.Speed > this._vehicleController.SpeedThreshold)
            {
                float boostToAdd = Time.deltaTime * this._rechargeRate * this._input.Brake;
                this._boostAmount = Mathf.Min(this._boostAmount + boostToAdd, _maxBoostAmount);
                if (this._boostAmount >= this._minimumActiveAmount)
                {
                    this._isBoostAvailable = true;
                }
            }
        }
    }
}