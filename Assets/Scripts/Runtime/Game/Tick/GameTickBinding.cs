using System;


namespace Momentum
{


    public struct Tick      : IEvent {}
    public struct TickFixed : IEvent {}
    public struct TickLate  : IEvent {}


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


    public class WeakSubscriber<T> where T : IEvent
    {
        private readonly WeakReference targetRef;
        private readonly Action callback;
        private readonly EventBinding<T> binding;

        public WeakSubscriber(EventBinding<T> binding, Action callback, object target)
        {
            targetRef       = new WeakReference(target);
            this.callback   = callback;
            this.binding    = binding;

            binding.Add(OnTick);
        }

        private void OnTick()
        {
            if (!targetRef.IsAlive)
            {
                binding.Remove(OnTick);
                return;
            }
            callback?.Invoke();
        }
    }
}



