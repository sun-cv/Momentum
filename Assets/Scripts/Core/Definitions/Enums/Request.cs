

namespace Momentum
{

    public enum Request
    {
        Create,
        Destroy,
        Start,
        Stop,
        Enter,
        Exit,
        Set,
        Get,
        Clear,
        Queue,
        Interrupt,
        Override,
        Switch,
        Transition,
        Cancel
    }
    public enum Response
    {
        Success,
        Failure,
        Accepted,
        Rejected,
        Denied,
        Pending,
        Expired,
        Canceled,
        Blocked,
        Invalid
    }

}