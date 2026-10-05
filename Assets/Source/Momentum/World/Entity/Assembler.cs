using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Game.Common;
using Game.Diagnostic;

using Physics   = Game.Common.Physics;
using Collision = Game.Common.Collision;
using Animation = Game.Common.Animation;
using Pose      = Game.Common.Pose;
using UnityEngine.Identifiers;


namespace Game.Realm
{
    public partial class Entities
    {
        internal class Assembler
        {
            private readonly Pool Pool;
            private readonly Masks Mask;
            private readonly Components Component;
            private readonly Bodies Bodies;

            public List<Func<Definition, bool>> archetype = new()
            {
                (definition) => definition.Prop         is Prop, 
                (definition) => definition.Actor        is Actor, 
                (definition) => definition.Corpse       is Corpse, 
                (definition) => definition.Effect       is Effect,
                (definition) => definition.Hitbox       is Hitbox,
                (definition) => definition.Ability      is Ability,
                (definition) => definition.Cooldown     is Cooldown,
                (definition) => definition.Spawner      is Spawner, 
                (definition) => definition.Directive    is Directive,
                (definition) => definition.Projectile   is Projectile,
            };

            internal Assembler(Masks mask, Pool pool, Components component, Bodies bodies)
            {
                Pool        = pool;
                Mask        = mask;
                Component   = component;
                Bodies      = bodies;
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

                if (Component.Has<Form>(entity))
                {
                    Bodies.Remove(Component.View<Form>(entity).Body);
                }

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

                Component.Add<Meta>(entity, new() { Created = Watch.Tick.Game });
                Component.Add<Identity>(entity, new() { Id  = definition.Id });

                if (definition.Innate is Innate innate)                 Component.Add<Innate>(entity, innate);
                if (definition.Blocks is Blocks blocks)                 Component.Add<Blocks>(entity, blocks);
                if (definition.Actor is Actor)                          Component.Add<Actor>(entity, new());
                if (definition.Prop is Prop)                            Component.Add<Prop>(entity, new());
                if (definition.Corpse is Corpse)                        Component.Add<Corpse>(entity, new());
                if (definition.Spawner is Spawner)                      Component.Add<Spawner>(entity, new());
                if (definition.Projectile is Projectile)                Component.Add<Projectile>(entity, new());
                if (definition.Directive is Directive)                  Component.Add<Directive>(entity, new());
                if (definition.Effect is Effect)                        Component.Add<Effect>(entity, new());
                if (definition.Hitbox is Hitbox)                        Component.Add<Hitbox>(entity, new());
                if (definition.Cooldown is Cooldown)                    Component.Add<Cooldown>(entity, new());
                if (definition.Cooldowns is Cooldowns cooldowns)        Component.Add<Cooldowns>(entity, cooldowns);
                if (definition.CooldownTarget is CooldownTarget target) Component.Add<CooldownTarget>(entity, target);
                if (definition.Anchor is Anchor anchor)                 Component.Add<Anchor>(entity, anchor);
                if (definition.Duration is Duration duration)           Component.Add<Duration>(entity, duration);
                if (definition.Ledger is Ledger)                        Component.Add<Ledger>(entity, new());
                if (definition.Faction is Faction faction)              Component.Add<Faction>(entity, faction);
                if (definition.Allegiance is Allegiance allegiance)     Component.Add<Allegiance>(entity, allegiance);
                if (definition.Temperament is Temperament temperament)  Component.Add<Temperament>(entity, temperament);
                if (definition.Movement is Movement movement)           Component.Add<Movement>(entity, movement);
                if (definition.Physics is Physics physics)              Component.Add<Physics>(entity, physics);
                if (definition.Force is Force)                          Component.Add<Force>(entity, new());
                if (definition.Contact is Contact)                      Component.Add<Contact>(entity, new());
                if (definition.Collision is Collision)                  Component.Add<Collision>(entity, new());
                if (definition.Hitboxes is Hitboxes hitboxes)           Component.Add<Hitboxes>(entity, hitboxes);
                if (definition.Loadout is Loadout loadout)              Component.Add<Loadout>(entity, loadout);
                if (definition.Activation is Activation activation)     Component.Add<Activation>(entity, activation);
                if (definition.Controls is Controls Controls)           Component.Add<Controls>(entity, Controls);
                if (definition.Sustain is Sustain sustain)              Component.Add<Sustain>(entity, sustain);
                if (definition.Chains is Chains chains)                 Component.Add<Chains>(entity, chains);
                if (definition.Equipment is Equipment equipment)        Component.Add<Equipment>(entity, equipment);
                if (definition.Inventory is Inventory inventory)        Component.Add<Inventory>(entity, inventory);
                if (definition.Target is Target)                        Component.Add<Target>(entity, new());
                if (definition.Aim is Aim)                              Component.Add<Aim>(entity, new());
                if (definition.Health is Health health)                 Component.Add<Health>(entity, health);
                if (definition.Armor is Armor armor)                    Component.Add<Armor>(entity, armor);
                if (definition.Energy is Energy energy)                 Component.Add<Energy>(entity, energy);
                if (definition.Displacement is Displacement displace)   Component.Add<Displacement>(entity, displace);
                if (definition.CameraTarget is CameraTarget)            Component.Add<CameraTarget>(entity, new());
                if (definition.Pose is Pose pose)                       Component.Add<Pose>(entity, pose);

                if (definition.Ability is Ability) 
                {
                    Component.Add<Ability>(entity, new());

                    if (definition.Phases is Phases phases)
                    {
                        Component.Add<Phases>(entity, phases);
                        Component.Add<Phase>(entity, new() { Index = 0, Elapsed = 0 });
                    }
                }

                if (definition.Mass is Mass mass)
                {
                    Component.Add<Mass>(entity, mass);
                    Component.Add<Facing>(entity, new() { Direction = Vector2.right });
                    Component.Add<Kinematic>(entity, new());
                    Component.Add<Control>(entity, new() { Modifier = new() });
                    Component.Add<Impulse>(entity, new());
                    Component.Add<Velocity>(entity, new());
                    Component.Add<TimeScale>(entity, new() { Scale = 1});
                }

                if (definition.PlayerController is PlayerController)
                {
                    Component.Add<PlayerController>(entity, new());
                    Component.Add<Intent>(entity, new());
                    Component.Add<Commands>(entity, new() { Active = new(), Buffer = new() });
                } 

                if (definition.AiController is AiController)
                {
                    Component.Add<AiController>(entity, new());
                    Component.Add<Intent>(entity, new());
                    Component.Add<Commands>(entity, new() { Active = new(), Buffer = new() });
                }
            }

            private void ProcessPrefab(Entity entity, Definition definition, GameObject instance)
            {
                var nodes = new Dictionary<string, Transform>();

                Walk(nodes, instance.transform);

                Component.Add<Instance>(entity, new() { Transform = instance.transform });

                if (definition.Form is Form)
                {
                    var body = instance.GetComponent<Rigidbody2D>();

                    body.freezeRotation = true;
                    body.gravityScale   = 0;
                    body.interpolation  = RigidbodyInterpolation2D.None; 
                    body.bodyType       = RigidbodyType2D.Kinematic;

                    Component.Add<Form>(entity, new() { Body = body });
                    Bodies.Add(body, entity);
                }

                if (definition.Rendering is Rendering rendering)
                {
                    var renderer = CreateRenderer(definition, rendering, instance.transform);

                    Component.Add<Rendering>(entity, new() { Layer = rendering.Layer, Renderer = renderer });
                    Component.Add<Animation>(entity, new() { Sheet = string.Empty, State = string.Empty });

                    if (Component.Has<Form>(entity))
                    {
                        Component.Add<Visual>(entity, new() { Transform = renderer.transform, Current = instance.transform.position, Previous = instance.transform.position });
                    }
                }

                if (definition.HurtBox is HurtBox && nodes.TryGetValue("Hurt", out var hurtNode))
                {
                    Component.Add<HurtBox>(entity, new() { Collider = hurtNode.GetComponent<Collider2D>() });
                }
            }

            private void Walk(Dictionary<string, Transform> nodes, Transform transform)
            {
                nodes[transform.name] = transform;

                for (var i = 0; i < transform.childCount; i++)
                {
                    Walk(nodes, transform.GetChild(i));
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

            private SpriteRenderer CreateRenderer(Definition definition, Rendering rendering, Transform parent)
            {
                if (string.IsNullOrEmpty(rendering.Layer) || !SortingLayer.IsValid(SortingLayer.NameToID(rendering.Layer)))
                    throw new Exception($"[Assembler.Validation] {definition.Id}: Rendering layer '{rendering.Layer}' is not a sorting layer");

                var node     = new GameObject("Visual");
                var renderer = node.AddComponent<SpriteRenderer>();

                node.layer = parent.gameObject.layer;
                node.transform.SetParent(parent, false);

                renderer.sortingLayerID  = SortingLayer.NameToID(rendering.Layer);
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;

                return renderer;
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
                            throw new Exception($"[Assembler.Validation.Archetype] {definition.Id}: {message}");
                    }
                }

                foreach (var (matches, rules) in ComponentRequirement)
                {
                    if (!matches(Component, entity))
                        continue;

                    foreach (var (check, message) in rules)
                    {
                        if (!check(Component, entity))
                            throw new Exception($"[Assembler.Validation.Component] {definition.Id}: {message}");
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
                {
                    definition => definition.Ability is Ability, new()
                    {
                        ((Component, entity, definition) => Component.Has<Activation>(entity),  "Ability requires Activation"),
                        ((Component, entity, definition) => Component.Has<Phases>(entity),      "Ability requires Phases"),
                        ((Component, entity, definition) => Component.Has<Phase>(entity),       "Ability requires Phase"),
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
                {
                    (Component, entity) => Component.Has<Rendering>(entity), new()
                    {
                        ((Component, entity) => Component.Has<Pose>(entity),                    "Rendering requires Pose"),
                    }
                },
                {
                    (Component, entity) => Component.Has<Track>(entity), new()
                    {
                        ((Component, entity) => Component.Has<Aim>(entity),                     "Tracking requires Aim"),
                    }
                },
            };

            static Assembler() => Log<Assembler>.Level(Diagnostic.Log.Level.Debug);
        }
    }
}
