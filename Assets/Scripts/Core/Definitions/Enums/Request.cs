

namespace Momentum
{

    public enum Request
    {
        None,
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
        Execute,
        Cancel,
        Interrupt,
        Override,
        Switch,
        Transition,
    }

    public enum Response
    {
        None,
        Pending,
        Buffered,
        Success,
        Failure,
        Accepted,
        Rejected,
        Expired,
        Canceled,
        Invalid
    }

    public enum Command
    {
        None,
        Pend,
        Buffer,
        Accept,
        Reject,
        Expire,
        Cancel,
        Invalidate,
    }

}