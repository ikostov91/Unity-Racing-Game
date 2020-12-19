using UnityEngine;

public class VehiclePositionReset : MonoBehaviour
{
    [SerializeField] private Transform _startPosition;
    [SerializeField] private Transform _resetPosition;

    [SerializeField] private GameObject _vehicle;
    [SerializeField] private Global _global;

    private Rigidbody _vehicleRigidBody;

    void Start()
    {
        this._global = FindObjectOfType<Global>();
        this._vehicle = this._global.GetCurrentVehicle;
        this._vehicleRigidBody = this._vehicle.GetComponent<Rigidbody>();

        this.PlaceVehicleAtStartPosition();
    }

    void Update()
    {
        this.ResetVehicle();
    }

    private void PlaceVehicleAtStartPosition()
    {
        this._vehicle.transform.position = this._startPosition.transform.position;
        this._vehicle.transform.rotation = this._startPosition.transform.rotation;
    }

    private void ResetVehicle()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            this._vehicleRigidBody.velocity = Vector3.zero;
            this._vehicleRigidBody.angularVelocity = Vector3.zero;

            this._vehicle.transform.position = this._resetPosition.transform.position;
            this._vehicle.transform.rotation = this._resetPosition.transform.rotation;
        }
    }
}
