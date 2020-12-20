using Constants;
using UnityEngine;

public class SpeedLimiter : MonoBehaviour
{
    private const float _speedLimitKph = 60f;

    private EngineController _engineController;
    private VehicleController _vehicleController;

    private bool _limiterActive = false;

    void Start()
    {
        this._engineController = GetComponent<EngineController>();
        this._vehicleController = GetComponent<VehicleController>();    
    }

    void Update()
    {
        this.ManualActivation();
        this.ApplySpeedLimit();
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.gameObject.CompareTag(TagConstants.PlayerVehicleTag))
        {
            Debug.Log("Enter");
            this._limiterActive = true;
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.gameObject.CompareTag(TagConstants.PlayerVehicleTag))
        {
            Debug.Log("Exit");
            this._limiterActive = false;
        }
    }

    private void ManualActivation()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            this._limiterActive = !this._limiterActive;
        }
    }

    private void ApplySpeedLimit()
    {
        if (this._limiterActive)
        {
            if (this._vehicleController.Speed >= _speedLimitKph)
            {
                this._engineController.ThrottleCut = true;
            }
            else
            {
                this._engineController.ThrottleCut = false;
            }
        }
    }
}
