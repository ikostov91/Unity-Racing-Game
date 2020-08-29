using UnityEngine;

public class LeaveTrack : MonoBehaviour
{
    public void OnCollisionEnter(Collision otherCollider)
    {
        VehicleController playerVehicle = otherCollider.gameObject.GetComponent<VehicleController>();
        if (playerVehicle)
        {
            FindObjectOfType<LapTimer>().InvalidateLap();
        }
    }
}
