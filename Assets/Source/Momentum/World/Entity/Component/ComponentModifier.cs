using Game.Common;



namespace Game.Realm
{

    public readonly struct ComponentModifierRegistry
    {
        readonly Components components;

        internal ComponentModifierRegistry(Components components)
        {
            this.components = components;
        }

        public ref Health Health(Entity entity) => ref this.components.Access<Health>().Modify(entity);
        public ref Energy Energy(Entity entity) => ref this.components.Access<Energy>().Modify(entity);
    }
}
