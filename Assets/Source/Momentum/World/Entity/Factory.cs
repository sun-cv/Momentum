using Game.Common;



namespace Game.Realm
{
    public partial class Entities
    {
        public partial class Factory
        {
            readonly Actors actor;

            internal Factory(Entities entities)
            {
                actor = new(entities);
            } 

            public Entity Actor(Definition.Actor definiton)
            {
                return actor.Create(definiton);
            }

        }

        public partial class Factory
        {

            public sealed class Actors 
            {
                readonly Entities entity;
                readonly Components component;

                internal Actors(Entities entities)
                {
                    entity     = entities;
                    component  = entities.Component;
                } 

                public Entity Create(Definition.Actor definition)
                {
                    var id = entity.Allocate();
                    
                    component.Add<Meta>(id, new() { Id = definition.Id, Created = Watch.RealTick });
                    component.Add<Actor>(id, new());
                    component.Add<Physics>(id, new());
                    component.Add<Transform>(id, new());
                    component.Add<TimeScale>(id, new());

                    if (definition.PlayerController is PlayerController)
                    {
                        component.Add<Intent>(id, new());
                        component.Add<CommandQueue>(id, new() { Active = new(), Buffer = new() });
                        component.Add<PlayerController>(id, new());
                    }
                    if (definition.AiController is AiController)
                    {
                        component.Add<Intent>(id, new());
                        component.Add<CommandQueue>(id, new() { Active = new(), Buffer = new() });
                        component.Add<AiController>(id, new());
                    }

                    if (definition.Aim is Aim)
                        component.Add<Aim>(id, new());

                    if (definition.Health is Health health)
                        component.Add<Health>(id, health);

                    if (definition.Energy is Energy energy)
                        component.Add<Energy>(id, energy);

                    if (definition.Movement is Movement)
                        component.Add<Movement>(id, new());

                    return id;
                }
            }
        }
    }
}
