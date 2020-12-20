using System.Collections.Generic;

namespace Assets.Scripts
{
    public interface IGearbox
    {
        int NumberOfGears { get; }
        int LowestGear { get; }
        int HighestGear { get; }
        int NeutralGear { get; }
        int ReverseGear { get; }
        int FirstGear { get; }
        float KPD { get; }
        Dictionary<int, float> GearRatios { get; }
        float FinalDriveRatio { get; }
    }
}
