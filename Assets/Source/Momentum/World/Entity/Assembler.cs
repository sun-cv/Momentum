using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Game.Common;
using Game.Diagnostic;

using Physics   = Game.Common.Physics;
using Collision = Game.Common.Collision;
using Animation = Game.Common.Animation;



namespace Game.Realm
{
    public partial class Entities
    {
        internal class Assembler
        {
            private readonly Pool Pool;
            private readonly Masks Mask;
            private readonly Components Component;

            public List<Func<Definition, bool>> archetype = new()
            {
                (definition) => definition.Prop         is Prop, 
                (definition) => definition.Actor        is Actor, 
                (definition) => definition.Effect       is Effect,
                (definition) => definition.Corpse       is Corpse, 
                (definition) => definition.Spawner      is Spawner, 
                (definition) => definition.Directive    is Directive,
                (definition) => definition.Projectile   is Projectile,
            };

            internal Assembler(Masks mask, Pool pool, Components component)
            {
                Pool        = pool;
                Mask        = mask;
                Component   = component;
            }

            public Entity Assemble(Definition definition, Entity parent)
            {
                RequireAlive(parent, definition);

                var entity = Pool.Allocate();

                ProcessParent(entity, parent);
                Build(entity, definition);

                return entity;
            }

            public Entity Assemble(Blueprint blueprint, ConstructionParameter parameter)
            {
                RequireAlive(parameter.Parent, blueprint.Definition);

                var entity = Pool.Allocate();

                ProcessParent(entity, parameter.Parent);
                Build(entity, blueprint, parameter);

                return entity;
            }

            private void Build(Entity entity, Definition definition)
            {
                ProcessDefinition(entity, definition);
                ValidateEntity(entity, definition);
            }

            private void Build(Entity entity, Blueprint blueprint, ConstructionParameter parameter)
            {
                var instance = UnityEngine.Object.Instantiate(blueprint.Prefab, parameter.Position, Quaternion.Euler(parameter.Rotation));

                ProcessDefinition(entity, blueprint.Definition);
                ProcessPrefab(entity, blueprint.Definition, instance);
                ValidateEntity(entity, blueprint.Definition);
            }

            public void Release(Entity entity)
            {
                Guard(entity);

                if (Component.Has<Instance>(entity))
                {
                    UnityEngine.Object.Destroy(Component.View<Instance>(entity).Transform.gameObject);
                }

                if (Component.Has<Child>(entity))
                {
                    foreach (var child in Component.View<Child>(entity).Entities.ToList())
                    {
                        Release(child);
                    } 
                }

                if (Component.Has<Parent>(entity))
                {
                    Component.Modify.Child(Component.View<Parent>(entity).Entity).Entities.Remove(entity);
                }

                Mask.Release(entity);
                Pool.Release(entity);
            }

            private void ProcessDefinition(Entity entity, Definition definition)
            {
                GenerateComponents(entity, definition);
                AssignInnateCapabilities(entity, definition);
            }

            private void GenerateComponents(Entity entity, Definition definition)
            {

                Component.Add<Meta>(entity, definition.Meta);

                if (definition.Innate is Innate innate)                 Component.Add<Innate>(entity, innate);
                if (definition.Blocks is Blocks blocks)                 Component.Add<Blocks>(entity, blocks);
                if (definition.Actor is Actor)                          Component.Add<Actor>(entity, new());
                if (definition.Prop is Prop)                            Component.Add<Prop>(entity, new());
                if (definition.Corpse is Corpse)                        Component.Add<Corpse>(entity, new());
                if (definition.Spawner is Spawner)                      Component.Add<Spawner>(entity, new());
                if (definition.Projectile is Projectile)                Component.Add<Projectile>(entity, new());
                if (definition.Ledger is Ledger)                        Component.Add<Ledger>(entity, new());
                if (definition.Faction is Faction faction)              Component.Add<Faction>(entity, faction);
                if (definition.Allegiance is Allegiance allegiance)     Component.Add<Allegiance>(entity, allegiance);
                if (definition.Temperament is Temperament temperament)  Component.Add<Temperament>(entity, temperament);
                if (definition.Movement is Movement movement)           Component.Add<Movement>(entity, movement);
                if (definition.Physics is Physics physics)              Component.Add<Physics>(entity, physics);
                if (definition.Force is Force)                          Component.Add<Force>(entity, new());
                if (definition.Contact is Contact)                      Component.Add<Contact>(entity, new());
                if (definition.Collision is Collision)                  Component.Add<Collision>(entity, new());
                if (definition.Abilities is Abilities abilities)        Component.Add<Abilities>(entity, abilities);
                if (definition.Loadout is Loadout loadout)              Component.Add<Loadout>(entity, loadout);
                if (definition.Equipment is Equipment equipment)        Component.Add<Equipment>(entity, equipment);
                if (definition.Inventory is Inventory inventory)        Component.Add<Inventory>(entity, inventory);
                if (definition.Target is Target)                        Component.Add<Target>(entity, new());
                if (definition.Aim is Aim)                              Component.Add<Aim>(entity, new());
                if (definition.Health is Health health)                 Component.Add<Health>(entity, health);
                if (definition.Energy is Energy energy)                 Component.Add<Energy>(entity, energy);
                if (definition.Displacement is Displacement displace)   Component.Add<Displacement>(entity, displace);

                if (definition.Mass is Mass mass)
                {
                    Component.Add<Mass>(entity, mass);
                    Component.Add<Kinematic>(entity, new());
                    Component.Add<Control>(entity, new());
                    Component.Add<Impulse>(entity, new());
                    Component.Add<Velocity>(entity, new());
                    Component.Add<TimeScale>(entity, new() { Scale = 1});
                }


                if (definition.PlayerController is PlayerController)
                {
                    Component.Add<PlayerController>(entity, new());
                    Component.Add<Intent>(entity, new());
                    Component.Add<CommandQueue>(entity, new() { Active = new(), Buffer = new() });
                } 

                if (definition.AiController is AiController)
                {
                    Component.Add<AiController>(entity, new());
                    Component.Add<Intent>(entity, new());
                    Component.Add<CommandQueue>(entity, new() { Active = new(), Buffer = new() });
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
                {
                    Component.Add<Instance>(entity, new() { Transform = instance.transform });
                }

                if (definition.Body is Body && nodes.TryGetValue("Body", out var bodyNode))
                {
                    Component.Add<Body>(entity, new() { Form = bodyNode.GetComponent<Rigidbody2D>() });
                }

                if (definition.Animation is Animation && nodes.TryGetValue("Animator", out var animatorNode))
                {
                    Component.Add<Animation>(entity, new() { Animator = animatorNode.GetComponent<Animator>() });
                }

                if (definition.Rendering is Rendering && nodes.TryGetValue("Renderer", out var rendererNode))
                {
                    Component.Add<Rendering>(entity, new() { Renderer = rendererNode.GetComponent<SpriteRenderer>() });
                }

                if (definition.Sorting is Sorting && nodes.TryGetValue("Sort", out var sortNode))
                {
                    Component.Add<Sorting>(entity, new()
                    {
                        Layer = sortNode.GetComponent<Collider2D>(),
                        Front = nodes.TryGetValue("Front", out var front) ? front.GetComponent<Collider2D>() : null,
                        Back  = nodes.TryGetValue("Back", out var back)   ? back.GetComponent<Collider2D>()  : null,
                    });
                }

                if (definition.HurtBox is HurtBox && nodes.TryGetValue("Hurt", out var hurtNode))
                {
                    Component.Add<HurtBox>(entity, new() { Collider = hurtNode.GetComponent<Collider2D>() });
                }
            }

            private void ProcessParent(Entity child, Entity? instance)
            {
                if (instance is not Entity parent)
                    return;

                Component.Add<Parent>(child, new() { Entity = parent });
                
                if (!Component.Has<Child>(parent))
                {
                    Component.Add<Child>(parent, new() { Entities = new() });
                }

                Component.Modify.Child(parent).Entities.Add(child);
            }

            private void AssignInnateCapabilities(Entity entity, Definition definition)
            {
                Mask<Innate> mask = default;

                if (definition.Innate is Innate capabilities)
                {
                    foreach (var capability in capabilities.Capabilities)
                    {
                        mask = mask.With((int)capability);
                    }
                }

                Mask.Get<Innate>().Add(entity, mask);
            }

            private void Guard(Entity entity)
            {
                Pool.Guard(entity);
            }

            private void RequireAlive(Entity? parent, Definition definition)
            {
                if (parent is Entity entity)
                    Pool.Guard(entity);
            }

            private bool ValidateEntity(Entity entity, Definition definition)
            {
                int kinds = 0;

                foreach (var archetype in archetype)
                {
                    if (archetype(definition)) kinds++;
                }

                if (kinds != 1)
                    throw new Exception($"[Assembler.Validation] Entity {definition.Id} has {kinds} archetypes, expected 1");

                foreach (var (matches, rules) in ArchetypeComponentRequirement)
                {
                    if (!matches(definition))
                        continue;

                    foreach (var (check, message) in rules)
                    {
                        if (!check(Component, entity, definition))
                            throw new Exception($"[Assembler.Validation] {definition.Id}: {message}");
                    }
                }

                foreach (var (matches, rules) in ComponentRequirement)
                {
                    if (!matches(Component, entity))
                        continue;

                    foreach (var (check, message) in rules)
                    {
                        if (!check(Component, entity))
                            throw new Exception($"[Assembler.Validation] {definition.Id}: {message}");
                    }
                }
                return true;
            }

            private static readonly Dictionary<Func<Definition, bool>, List<(Func<Components, Entity, Definition, bool> Check, string Message)>> ArchetypeComponentRequirement = new()
            {
                {
                    definition => definition.Effect is Effect, new()
                    {
                        ((Component, entity, definition) => Component.Has<Parent>(entity),      "Effect requires Parent"),
                        ((Component, entity, definition) => Component.Has<Blocks>(entity),      "Effect requires Blocks"),
                        ((Component, entity, definition) => definition.Prefab is null,          "Effect must not name a prefab"),
                    }
                },
                {
                    definition => definition.Directive is Directive, new()
                    {
                        ((Component, entity, definition) => Component.Has<Parent>(entity),      "Directive requires Parent"),
                        ((Component, entity, definition) => Component.Has<Blocks>(entity),      "Directive requires Blocks"),
                        ((Component, entity, definition) => Component.Has<Displacement>(entity),"Directive requires Displacement"),
                        ((Component, entity, definition) => definition.Prefab is null,          "Directive must not name a prefab"),
                    }
                },
                {
                    definition => definition.Actor is Actor, new()
                    {
                        ((Component, entity, definition) => Component.Has<Instance>(entity),    "Actor requires Instance"),
                    }
                },
            };

            private static readonly Dictionary<Func<Components, Entity, bool>, List<(Func<Components, Entity, bool> Check, string Message)>> ComponentRequirement = new()
            {
                {
                    (Component, entity) => Component.Has<Movement>(entity), new()
                    {
                        ((Component, entity) => Component.Has<Mass>(entity),                    "Movable entity requires Mass component"),
                    }
                },
            };

            static Assembler() => Log<Assembler>.Level(Diagnostic.Log.Level.Debug);
        }
    }
}
