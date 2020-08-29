using UnityEngine;

public class Suspension : MonoBehaviour
{
    [SerializeField] private AxleInfo[] _axleInfos;
    [SerializeField] private float _antiRoll = 5000f;

    private VehicleController _vehicle;
    private Rigidbody _vehicleRigidBody;

    void Start()
    {
        this._vehicle = FindObjectOfType<VehicleController>();
        this._vehicleRigidBody = this._vehicle.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        this.AntiRoll();
    }

    private void AntiRoll()
    {
        foreach (AxleInfo axle in this._axleInfos)
        {
            this.ApplyAntiRollToWheels(axle);
        }
    }

    private void ApplyAntiRollToWheels(AxleInfo axle)
    {
        WheelHit hit;
        float travelLeft = 1.0f;
        float travelRight = 1.0f;

        bool groundedLeft = axle.LeftWheel.GetGroundHit(out hit);
        if (groundedLeft)
        {
            // Calculate suspension travel
            travelLeft = (-axle.LeftWheel.transform.InverseTransformPoint(hit.point).y - axle.LeftWheel.radius) / axle.LeftWheel.suspensionDistance;
        }
        bool groundedRight = axle.RightWheel.GetGroundHit(out hit);
        if (groundedRight)
        {
            travelRight = (-axle.RightWheel.transform.InverseTransformPoint(hit.point).y - axle.RightWheel.radius) / axle.RightWheel.suspensionDistance;
        }

        float antiRollForce = (travelLeft - travelRight) * this._antiRoll;

        if (groundedLeft)
        {
            this._vehicleRigidBody.AddForceAtPosition(axle.LeftWheel.transform.up * -antiRollForce, axle.LeftWheel.transform.position);
        }
        if (groundedRight)
        {
            this._vehicleRigidBody.AddForceAtPosition(axle.RightWheel.transform.up * antiRollForce, axle.RightWheel.transform.position);
        }
    }
}
