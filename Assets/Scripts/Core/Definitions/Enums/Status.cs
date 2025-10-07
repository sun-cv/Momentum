

namespace Momentum
{

    public enum Status
    {
        None,
        Active,
        Inactive,
        Disabled,
        Expired,
    }

    public enum Lifecycle
    {
        None,
        Queued,
        Running,
        Paused,
        Completed,
        Failed,
        Cancelled,
        Interrupted,
    }


}