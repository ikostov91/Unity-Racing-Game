using System.Collections.Generic;

namespace Assets.Scripts
{
    public interface IEngine
    {
        int MinimumRpm { get; }
        int MaximumRmp { get; }
        int RedLine { get; }
        int UpShiftRpm { get; }
        int DownShiftRpm { get; }
        float Inertia { get; }
        float Friction { get; }
        Dictionary<int, float> TorqueCurve { get; }
    }
}
