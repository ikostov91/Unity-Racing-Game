using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Gearbox
{
    public int NumberOfGears = 5;

    public int LowestGear = -1;
    public int HighestGear = 5;

    public int NeutralGear = 0;
    public int ReverseGear = -1;
    public int FirstGear = 1;

    public float KPD = 0.96f;

    public Dictionary<int, float> ForwardGearRatios = new Dictionary<int, float>()
    {
        { 0, 0f },
        { 1, 4.2f },
        { 2, 2.49f },
        { 3, 1.67f },
        { 4, 1.24f },
        { 5, 1f },
    };

    public float FinalDriveRatio = 2.93f;

    public float ReverseRatio = 3.89f;
}
