using Game.Common;



namespace Game.Realm
{

    public partial class Components
    {        
        public sealed class Modifier
        {
            readonly Components components;

            internal Modifier(Components components)
            {
                this.components = components;
            }

            public ref Health Health  (Entity entity) => ref components.Access<Health>() .Reference(entity);
            public ref Energy Energy  (Entity entity) => ref components.Access<Energy>() .Reference(entity);
            public ref Intent Intent  (Entity entity) => ref components.Access<Intent>() .Reference(entity);
            public ref Aim Aim        (Entity entity) => ref components.Access<Aim>()    .Reference(entity);
            public ref Command Command(Entity entity) => ref components.Access<Command>().Reference(entity);
        }
    }


    public partial class Entities
    {
        public Health Health  (Entity entity) => component.View<Health>(entity);
        public Energy Energy  (Entity entity) => component.View<Energy>(entity);
        public Intent Intent  (Entity entity) => component.View<Intent>(entity);
        public Aim Aim        (Entity entity) => component.View<Aim>(entity);
        public Command Command(Entity entity) => component.View<Command>(entity);
    }


}
