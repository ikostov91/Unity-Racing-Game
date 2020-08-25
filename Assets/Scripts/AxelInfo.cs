using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider LeftWheel;
    public WheelCollider RightWheel;
    public bool Motor; // is this wheel attached to motor?
    public bool Steering; // does this wheel apply steer angle?
    public float BrakeBias;
}
