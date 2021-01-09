using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DriftGearbox : MonoBehaviour, IGearbox
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
        { -1, 2.8f },
        {  0, 0f    },
        {  1, 3.133f },
        {  2, 2.045f },
        {  3, 1.481f },
        {  4, 1.161f },
        {  5, 0.971f },
        {  6, 0.811f }
    };

    public float FinalDriveRatio => 4.756f;
}
