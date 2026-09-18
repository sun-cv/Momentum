using System.Collections.Generic;
using UnityEngine;
using Game.Common;

using Physics   = Game.Common.Physics;
using Collision = Game.Common.Collision;
using Animation = Game.Common.Animation;


namespace Game.Realm
{
    public partial class Entities
    {
        internal class Assembler
        {
            private readonly Entities entities;
            private readonly Components component;

            internal Assembler(Entities entities)
            {
                this.entities = entities;
                this.component = entities.Component;
            }

            public Entity Assemble(Blueprint blueprint, Vector3 position)
            {
                var entity      = entities.Allocate();

                var prefab      = blueprint.Prefab;
                var definition  = blueprint.Definition;
                var instance    = Object.Instantiate(prefab, position, Quaternion.identity);

                ProcessDefinition(entity, definition);
                ProcessPrefab(entity, definition, instance);

                return entity;
            }

            public void Dismantle(Entity entity)
            {
                if (component.Has<Instance>(entity))
                    Object.Destroy(component.View<Instance>(entity).Transform.gameObject);

                component.Clear(entity);
            }

            private void ProcessDefinition(Entity entity, Definition definition)
            {
                component.Add<Meta>(entity, definition.Meta);

                if (definition.Actor is Actor)                          component.Add<Actor>(entity, new());
                if (definition.Prop is Prop)                            component.Add<Prop>(entity, new());
                if (definition.Corpse is Corpse)                        component.Add<Corpse>(entity, new());
                if (definition.Spawner is Spawner)                      component.Add<Spawner>(entity, new());
                if (definition.Projectile is Projectile)                component.Add<Projectile>(entity, new());
                if (definition.Ledger is Ledger)                        component.Add<Ledger>(entity, new());
                if (definition.Faction is Faction faction)              component.Add<Faction>(entity, faction);
                if (definition.Allegiance is Allegiance allegiance)     component.Add<Allegiance>(entity, allegiance);
                if (definition.Temperament is Temperament temperament)  component.Add<Temperament>(entity, temperament);
                if (definition.Physics is Physics physics)              component.Add<Physics>(entity, physics);
                if (definition.Force is Force force)                    component.Add<Force>(entity, force);
                if (definition.Contact is Contact contact)              component.Add<Contact>(entity, contact);
                if (definition.Collision is Collision collision)        component.Add<Collision>(entity, collision);
                if (definition.Movement is Movement movement)           component.Add<Movement>(entity, movement);
                if (definition.Abilities is Abilities abilities)        component.Add<Abilities>(entity, abilities);
                if (definition.Loadout is Loadout loadout)              component.Add<Loadout>(entity, loadout);
                if (definition.Equipment is Equipment equipment)        component.Add<Equipment>(entity, equipment);
                if (definition.Inventory is Inventory inventory)        component.Add<Inventory>(entity, inventory);
                if (definition.Target is Target target)                 component.Add<Target>(entity, target);
                if (definition.Aim is Aim aim)                          component.Add<Aim>(entity, aim);
                if (definition.Health is Health health)                 component.Add<Health>(entity, health);
                if (definition.Energy is Energy energy)                 component.Add<Energy>(entity, energy);
                if (definition.TimeScale is TimeScale timeScale)        component.Add<TimeScale>(entity, timeScale);

                if (definition.PlayerController is PlayerController)
                {
                    component.Add<PlayerController>(entity, new());
                    component.Add<Intent>(entity, new());
                    component.Add<CommandQueue>(entity, new() { Active = new(), Buffer = new() });
                }

                if (definition.AiController is AiController)
                {
                    component.Add<AiController>(entity, new());
                    component.Add<Intent>(entity, new());
                    component.Add<CommandQueue>(entity, new() { Active = new(), Buffer = new() });
                }
            }

            private void ProcessPrefab(Entity entity, Definition definition, GameObject instance)
            {
                var nodes = new Dictionary<string, Transform>();

                void Walk(Transform transform)
                {
                    nodes[transform.name] = transform;

                    for (var i = 0; i < transform.childCount; i++)
                        Walk(transform.GetChild(i));
                }
                Walk(instance.transform);

                if (definition.Instance is Instance)
                    component.Add<Instance>(entity, new() { Transform = instance.transform });

                if (definition.Body is Body && nodes.TryGetValue("Body", out var bodyNode))
                    component.Add<Body>(entity, new() { Form = bodyNode.GetComponent<Rigidbody2D>() });

                if (definition.Animation is Animation && nodes.TryGetValue("Animator", out var animatorNode))
                    component.Add<Animation>(entity, new() { Animator = animatorNode.GetComponent<Animator>() });

                if (definition.Renderering is Renderering && nodes.TryGetValue("Renderer", out var rendererNode))
                    component.Add<Renderering>(entity, new() { Renderer = rendererNode.GetComponent<SpriteRenderer>() });

                if (definition.Sorting is Sorting && nodes.TryGetValue("Sort", out var sortNode))
                    component.Add<Sorting>(entity, new()
                    {
                        Layer = sortNode.GetComponent<Collider2D>(),
                        Front = nodes.TryGetValue("Front", out var front) ? front.GetComponent<Collider2D>() : null,
                        Back  = nodes.TryGetValue("Back", out var back)   ? back.GetComponent<Collider2D>()  : null,
                    });

                if (definition.HurtBox is HurtBox && nodes.TryGetValue("Hurt", out var hurtNode))
                    component.Add<HurtBox>(entity, new() { Collider = hurtNode.GetComponent<Collider2D>() });
            }
        }
    }
}
