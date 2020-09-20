using System;
using UnityEngine;

[Serializable]
public class AxleInfo
{
    public WheelCollider LeftWheel;
    public WheelCollider RightWheel;
    public bool Motor;
    public bool Steering;
    public bool HandBrake;
    public float BrakeBias;
    public float TorqueBias;
    public WheelCollider[] GetAxleWheels() => new WheelCollider[] { this.LeftWheel, this.RightWheel };
}
