

namespace Momentum.Abilities
{


    public enum Phase
    {
        None,
        Activating,
        Active,
        Executing,
        Completing,  
        Completed,
        Cancelled,
        Interrupted,
        Deactivating,
        Deactivated
    }

    public enum CastPhase
    {
        None,
        Starting,
        Casting,
        Completing,
        Completed,
        Cancelled,
        Interrupted
    }

}