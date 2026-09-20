using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Game.Common;
using Game.Diagnostic;

using Physics   = Game.Common.Physics;
using Collision = Game.Common.Collision;
using Animation = Game.Common.Animation;
using System;

namespace Game.Realm
{
    public partial class Entities
    {
        internal class Assembler
        {
            private readonly Entities   Entity;
            private readonly Masks      Mask;

            internal Assembler(Entities entities)
            {
                Entity      = entities;
                Mask        = entities.Mask;
            }

            public Entity Assemble(Blueprint blueprint, Vector3 position)
            {
                var entity = Entity.Allocate();

                Build(entity, blueprint, position);

                return entity;
            }

            public Entity Assemble(Blueprint blueprint, Vector3 position, Entity parent)
            {
                if (!Entity.Alive(parent))
                    throw new Exception($"[Assembler] Parent is dead for {blueprint.Definition.Id}");

                var entity = Entity.Allocate();

                ProcessParent(entity, parent);
                Build(entity, blueprint, position);

                return entity;
            }

            private void Build(Entity entity, Blueprint blueprint, Vector3 position)
            {
                var instance = UnityEngine.Object.Instantiate(blueprint.Prefab, position, Quaternion.identity);

                ProcessDefinition(entity, blueprint.Definition);
                ProcessPrefab(entity, blueprint.Definition, instance);
            }

            public void Dismantle(Entity entity)
            {
                if (!Entity.Alive(entity))
                    throw new Exception($"[Assembler] Attemped to release dead entity");

                if (Entity.Component.Has<Instance>(entity))
                {
                    UnityEngine.Object.Destroy(Entity.Component.View<Instance>(entity).Transform.gameObject);
                }

                if (Entity.Component.Has<Child>(entity))
                {
                    foreach (var child in Entity.Component.View<Child>(entity).Entities.ToList())
                    {
                        Entity.Release(child);
                    }
                }

                if (Entity.Component.Has<Parent>(entity))
                {
                    Entity.Component.Modify.Child(Entity.Parent(entity).Entity).Entities.Remove(entity);
                }

                Entity.Component.Clear(entity);
            }

            private void ProcessDefinition(Entity entity, Definition definition)
            {
                Entity.Component.Add<Meta>(entity, definition.Meta);

                ProcessCapabilities(entity, definition);

                if (definition.Innate is Innate innate)                 Entity.Component.Add<Innate>(entity, innate);
                if (definition.Blocks is Blocks blocks)                 Entity.Component.Add<Blocks>(entity, blocks);
                if (definition.Actor is Actor)                          Entity.Component.Add<Actor>(entity, new());
                if (definition.Prop is Prop)                            Entity.Component.Add<Prop>(entity, new());
                if (definition.Corpse is Corpse)                        Entity.Component.Add<Corpse>(entity, new());
                if (definition.Spawner is Spawner)                      Entity.Component.Add<Spawner>(entity, new());
                if (definition.Projectile is Projectile)                Entity.Component.Add<Projectile>(entity, new());
                if (definition.Ledger is Ledger)                        Entity.Component.Add<Ledger>(entity, new());
                if (definition.Faction is Faction faction)              Entity.Component.Add<Faction>(entity, faction);
                if (definition.Allegiance is Allegiance allegiance)     Entity.Component.Add<Allegiance>(entity, allegiance);
                if (definition.Temperament is Temperament temperament)  Entity.Component.Add<Temperament>(entity, temperament);
                if (definition.Physics is Physics physics)              Entity.Component.Add<Physics>(entity, physics);
                if (definition.Force is Force force)                    Entity.Component.Add<Force>(entity, force);
                if (definition.Contact is Contact contact)              Entity.Component.Add<Contact>(entity, contact);
                if (definition.Collision is Collision collision)        Entity.Component.Add<Collision>(entity, collision);
                if (definition.Movement is Movement movement)           Entity.Component.Add<Movement>(entity, movement);
                if (definition.Abilities is Abilities abilities)        Entity.Component.Add<Abilities>(entity, abilities);
                if (definition.Loadout is Loadout loadout)              Entity.Component.Add<Loadout>(entity, loadout);
                if (definition.Equipment is Equipment equipment)        Entity.Component.Add<Equipment>(entity, equipment);
                if (definition.Inventory is Inventory inventory)        Entity.Component.Add<Inventory>(entity, inventory);
                if (definition.Target is Target target)                 Entity.Component.Add<Target>(entity, target);
                if (definition.Aim is Aim aim)                          Entity.Component.Add<Aim>(entity, aim);
                if (definition.Health is Health health)                 Entity.Component.Add<Health>(entity, health);
                if (definition.Energy is Energy energy)                 Entity.Component.Add<Energy>(entity, energy);
                if (definition.TimeScale is TimeScale timeScale)        Entity.Component.Add<TimeScale>(entity, timeScale);

                if (definition.PlayerController is PlayerController)
                {
                    Entity.Component.Add<PlayerController>(entity, new());
                    Entity.Component.Add<Intent>(entity, new());
                    Entity.Component.Add<CommandQueue>(entity, new() { Active = new(), Buffer = new() });
                } 

                if (definition.AiController is AiController)
                {
                    Entity.Component.Add<AiController>(entity, new());
                    Entity.Component.Add<Intent>(entity, new());
                    Entity.Component.Add<CommandQueue>(entity, new() { Active = new(), Buffer = new() });
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
                    Entity.Component.Add<Instance>(entity, new() { Transform = instance.transform });

                if (definition.Body is Body && nodes.TryGetValue("Body", out var bodyNode))
                    Entity.Component.Add<Body>(entity, new() { Form = bodyNode.GetComponent<Rigidbody2D>() });

                if (definition.Animation is Animation && nodes.TryGetValue("Animator", out var animatorNode))
                    Entity.Component.Add<Animation>(entity, new() { Animator = animatorNode.GetComponent<Animator>() });

                if (definition.Renderering is Renderering && nodes.TryGetValue("Renderer", out var rendererNode))
                    Entity.Component.Add<Renderering>(entity, new() { Renderer = rendererNode.GetComponent<SpriteRenderer>() });

                if (definition.Sorting is Sorting && nodes.TryGetValue("Sort", out var sortNode))
                    Entity.Component.Add<Sorting>(entity, new()
                    {
                        Layer = sortNode.GetComponent<Collider2D>(),
                        Front = nodes.TryGetValue("Front", out var front) ? front.GetComponent<Collider2D>() : null,
                        Back  = nodes.TryGetValue("Back", out var back)   ? back.GetComponent<Collider2D>()  : null,
                    });

                if (definition.HurtBox is HurtBox && nodes.TryGetValue("Hurt", out var hurtNode))
                    Entity.Component.Add<HurtBox>(entity, new() { Collider = hurtNode.GetComponent<Collider2D>() });
            }


            private void ProcessParent(Entity child, Entity? instance)
            {
                if (instance is not Entity parent)
                    return;

                Entity.Component.Add<Parent>(child, new() { Entity = parent });
                
                if (!Entity.Component.Has<Child>(parent))
                    Entity.Component.Add<Child>(parent, new() { Entities = new() });

                Entity.Component.Modify.Child(parent).Entities.Add(child);
            }


            private void ProcessCapabilities(Entity entity, Definition definition)
            {
                Mask<Innate> mask = default;

                if (definition.Innate is Innate capabilities)
                    foreach (var capability in capabilities.Capabilities)
                        mask = mask.With((int)capability);

                Mask.Get<Innate>().Set(entity, mask);
            }

            static Assembler() => Log<Assembler>.Level(Diagnostic.Log.Level.Debug);
        }
    }
}
