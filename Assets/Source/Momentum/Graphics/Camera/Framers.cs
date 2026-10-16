using System.Collections.Generic;

using Game.Common;
using Game.Common.Events;
using Game.Realm;



namespace Game.Graphics
{

    public class Framer
    {
        private readonly List<FrameRequest> requests = new();

        private int order;

        public Framer()
        {
            Event.Register<Framer, FrameCamera>();
            Event.Register<Framer, StopFrameCamera>();
        }

        public bool Tick(World world, out Framing framing, out int ticks)
        {
            var before = Top();

            foreach (var message in Event.Read<Framer, FrameCamera>())
                Start(message);

            foreach (var message in Event.Read<Framer, StopFrameCamera>())
                Stop(message.Id, message.Owner);

            Prune(world);

            var after = Top();

            framing = after is null ? Framing.Explore : after.Framing;
            ticks   = 0;

            if (after == before)
                return false;

            ticks = after is not null && (before is null || after.Order > before.Order) ? after.In : before.Out;
            return true;
        }

        private void Start(FrameCamera message)
        {
            Stop(message.Id, message.Owner);

            requests.Add(new FrameRequest(message, ++order));
        }

        private void Stop(string id, Entity? owner)
        {
            for (var i = requests.Count - 1; i >= 0; i--)
            {
                if (requests[i].Matches(id, owner))
                    requests.RemoveAt(i);
            }
        }

        private void Prune(World world)
        {
            for (var i = requests.Count - 1; i >= 0; i--)
            {
                if (requests[i].Orphaned(world))
                    requests.RemoveAt(i);
            }
        }

        private FrameRequest Top()
        {
            return requests.Count > 0 ? requests[requests.Count - 1] : null;
        }
    }

    public class FrameRequest
    {
        private readonly string  id;
        private readonly Entity? owner;

        public FrameRequest(FrameCamera message, int order)
        {
            id      = message.Id;
            owner   = message.Owner;
            Framing = message.Framing;
            In      = message.In;
            Out     = message.Out;
            Order   = order;
        }

        public Framing Framing  { get; }
        public int     In       { get; }
        public int     Out      { get; }
        public int     Order    { get; }

        public bool Matches(string other, Entity? source)
        {
            return !string.IsNullOrEmpty(other) && id == other && owner == source;
        }

        public bool Orphaned(World world)
        {
            return owner is Entity entity && !world.Entity.Alive(entity);
        }
    }
}
