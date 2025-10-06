

namespace Momentum
{
    public enum Request
    {
        Set,
        Get,
        Start,
        Stop,
        Enter,
        Exit,
        Create,
        Destroy,
        Clear,
        Queue,
        Interrupt,
        Override,
        Switch,
        Transition,
        Request,
        Cancel,
    }


    public enum Response
    {
        Error,
        Success,
        Failure,
        Pending,
        Denied,
        Canceled,
        Blocked,
    }
}