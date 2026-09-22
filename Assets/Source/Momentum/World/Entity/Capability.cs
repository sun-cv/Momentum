using Game.Common;



namespace Game.Realm
{
    public class Capabilities
    {
        private readonly MaskSet<Innate>     innate;
        private readonly MaskSet<Capability> active;

        internal Capabilities(Masks masks)
        {
            innate = masks.Get<Innate>();
            active = masks.Get<Capability>();
        }

        public bool Can(Entity entity, Capability capability)
        {
            return active.View(entity).Contains(new Mask<Capability>().With((int)capability));
        }

        public bool Innate(Entity entity, Capability capability)
        {
            return innate.View(entity).Contains(new Mask<Innate>().With((int)capability));
        }

        public void Reset(Entity entity)
        {
            active.Add(entity, innate.View(entity).To<Capability>());
        }

        public void Block(Entity entity, Capability capability)
        {
            active.Add(entity, active.View(entity).Without((int)capability));
        }
    }
}
