using UnityEngine;

public class DownforceController : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private VehicleController _vehicleController;

    [SerializeField] [Range(0.5f, 600f)] private float _downforce = 1.0f;

    void Start()
    {
        this._rigidBody = GetComponent<Rigidbody>();
        this._vehicleController = GetComponent<VehicleController>();
    }

    void FixedUpdate()
    {
        this.AddDownforce();
    }

    private void AddDownforce()
    {
        this._rigidBody.AddForce(-transform.up * this._downforce * this._vehicleController.Speed);
    }
}
