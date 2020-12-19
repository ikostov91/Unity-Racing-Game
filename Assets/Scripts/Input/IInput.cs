namespace Assets.Scripts.PlayerInput
{
    public interface IInput
    {
        float Throttle { get; }
        float Brake { get; }
        float Clutch { get; }
        float Steering { get; }
        bool GearUp { get; }
        bool GearDown { get; }
        bool Handbrake { get; }
        bool Boost { get; }
    }
}

