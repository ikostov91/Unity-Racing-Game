using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffTrack : MonoBehaviour
{

    public void OnCollisionEnter(Collision otherCollider)
    {
        VehicleController playerVehicle = otherCollider.gameObject.GetComponent<VehicleController>();
        if (playerVehicle)
        {
            Debug.Log("LAP INVALIDATED");
        }
    }
}
