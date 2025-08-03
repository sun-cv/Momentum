

namespace Momentum
{

public static class GameTickBinding
{
    public static readonly EventBinding<Tick>      Tick         = new();
    public static readonly EventBinding<TickLate>  TickLate     = new();
    public static readonly EventBinding<TickFixed> TickFixed    = new();

    public static void Register()
    {
        EventBus<Tick>     .Register(Tick);
        EventBus<TickLate> .Register(TickLate);
        EventBus<TickFixed>.Register(TickFixed);
    }

    public static void Deregister()
    {
        EventBus<Tick>     .Deregister(Tick);
        EventBus<TickLate> .Deregister(TickLate);
        EventBus<TickFixed>.Deregister(TickFixed);
    }

}


}