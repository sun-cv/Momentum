

namespace Momentum.Abilities
{
    public enum Token
    {
        Interaction,
        Movement,
        Offense,
        Defense,
        Aim,
        Cast,
        Channel,
        Instant,
    }

    public enum TokenState
    {
        Free,
        Reserved,
        Active,
    }
}