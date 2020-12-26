using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RallyGearbox : MonoBehaviour, IGearbox
{
    public int NumberOfGears => 6;
    public int LowestGear => -1;
    public int HighestGear => 6;
    public int NeutralGear => 0;
    public int ReverseGear => -1;
    public int FirstGear => 1;
    public float KPD => 0.96f;

    public Dictionary<int, float> GearRatios => new Dictionary<int, float>()
    {
        { -1, 3.89f },
        {  0, 0f    },
        {  1, 2.94f  },
        {  2, 2.33f },
        {  3, 1.88f },
        {  4, 1.55f },
        {  5, 1.3f    },
        {  6, 1.1f    },
    };

    public float FinalDriveRatio => 4.1f;
}