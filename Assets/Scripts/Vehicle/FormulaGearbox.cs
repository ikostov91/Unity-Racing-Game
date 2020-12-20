using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FormulaGearbox : MonoBehaviour, IGearbox
{
    public int NumberOfGears => 7;
    public int LowestGear => -1;
    public int HighestGear => 7;
    public int NeutralGear => 0;
    public int ReverseGear => -1;
    public int FirstGear => 1;
    public float KPD => 0.96f;

    public Dictionary<int, float> GearRatios => new Dictionary<int, float>()
    {
        { -1, 2.90f },
        {  0, 0f    },
        {  1, 2.97f },
        {  2, 2.07f },
        {  3, 1.43f },
        {  4, 1.00f },
        {  5, 0.71f },
        {  6, 0.57f },
        {  7, 0.48f }
    };

    public float FinalDriveRatio => 3.42f;
}
