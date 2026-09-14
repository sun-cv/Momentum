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

                    if (definition.Health is Health health)
                        component.Add<Health>(id, health);

                    if (definition.Energy is Energy energy)
                        component.Add<Energy>(id, energy);

                    if (definition.Intent is Intent intent)
                        component.Add<Intent>(id, intent);

                    if (definition.Aim is Aim aim)
                        component.Add<Aim>(id, aim);
                    
                    if (definition.Pushable is Pushable pushable)
                        component.Add<Pushable>(id, pushable);

                    if (definition.PlayerController is PlayerController controller)
                        component.Add<PlayerController>(id, controller);


                    return id;
                }
            }
        }
    }
}
