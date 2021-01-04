using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DriftGearbox : MonoBehaviour, IGearbox
{
    public int NumberOfGears => 5;
    public int LowestGear => -1;
    public int HighestGear => 5;
    public int NeutralGear => 0;
    public int ReverseGear => -1;
    public int FirstGear => 1;
    public float KPD => 0.96f;

    public Dictionary<int, float> GearRatios => new Dictionary<int, float>()
    {
        { -1, 2.90f },
        {  0, 0f    },
        {  1, 2.27f },
        {  2, 1.55f },
        {  3, 1.22f },
        {  4, 1.00f },
        {  5, 0.88f }
    };

    public float FinalDriveRatio => 3.61f;
}
