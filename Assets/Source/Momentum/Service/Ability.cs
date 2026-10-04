using System;
using System.Linq;
using System.Collections.Generic;

using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;



namespace Game.Service
{

    public class AbilitySystem : RegisteredService, IWorld, IData, IGameBase
    {
        private readonly Data Data;
        private readonly World World;

        private readonly List<Entity>     cancel    = new();
        private readonly List<Entity>     release   = new();
        private readonly List<Entity>     windows   = new();
        private readonly List<Capability> keys      = new();
        private readonly List<Capability> reserved  = new();

        public AbilitySystem(World world, Data data)
        {
            Data    = data;
            World   = world;
        }

        public void Tick()
        {
        
            Process();
        }

        private void Process()
        {
            AdvanceAbilities();
            ProcessAbilities();
            ReleaseAbilities();
        }

        // Advance
        private void AdvanceAbilities()
        {
            foreach (var entity in World.Query(Mask<Components, Ability, Phase>.Key))
            {
                AdvanceAbility(entity);
            }
        }

        private void AdvanceAbility(Entity ability)
        {
            if (SustainBroken(ability))
            {
                release.Add(ability);
                return;
            }

            if (PhaseSustainReleased(ability) || PhaseDurationElapsed(ability))
            {
                AdvancePhase(ability);
                ProcessCooldowns(ability);
            }

            if (Finished(ability))
            {
                Deactivate(ability);
                return;
            }
            
            Advance(ability);
        }

        private void AdvancePhase(Entity ability)
        {
            World.Entity.Modify.Phase(ability).Index++;
            World.Entity.Modify.Phase(ability).Elapsed = 0;
        }

        private void Advance(Entity ability)
        {
            World.Entity.Modify.Phase(ability).Elapsed++;
        }

        private void Deactivate(Entity ability)
        {
            if (World.Entity.Has<Chains>(ability) && World.Entity.Chains(ability).Window > 0)
            {
                windows.Add(ability);
                return;
            }
            
            release.Add(ability);
        }

        // Process
        private void ProcessAbilities()
        {
            OpenWindows();

            foreach (var entity in World.Query(Mask<Components, Commands, Loadout>.Key))
            {
                ClearReserved();
                ProcessChained(entity);
                ProcessDefault(entity);
            }
        }
    
        private void OpenWindows()
        {
            foreach (var ability in windows)
            {
                World.Entity.Component.Remove<Phase>(ability);

                if (World.Entity.Has<Pose>(ability))
                {
                    World.Entity.Component.Remove<Pose>(ability);
                }

                World.Entity.Component.Add(ability, new Duration { Length = World.Entity.Chains(ability).Window });
                World.Entity.Component.Add(ability, new ControlWindow());
            }

            windows.Clear();
        }        

        private void ClearReserved()
        {
            reserved.Clear();
        }

        private void ProcessChained(Entity entity)
        {
            var command = World.Entity.Command(entity);

            keys.Clear();
            keys.AddRange(command.Active.Keys);
            keys.AddRange(command.Buffer.Keys);

            foreach (var capability in keys)
            {
                if (!ResolveChained(entity, capability, out var id, out var chained))
                    continue;

                reserved.Add(capability);

                if (!ProcessAbility(entity, capability, Data.Definition(id)))
                    continue;

                if (World.Entity.Has<ControlWindow>(chained))
                    release.Add(chained);
            }
        }

        private bool ResolveChained(Entity entity, Capability capability, out string id, out Entity chained)
        {
            foreach (var ability in World.Query(Mask<Components, Chains, Parent>.Key))
            {
                if (World.Entity.Parent(ability).Entity != entity)
                    continue;

                if (World.Entity.Chains(ability).Abilities.TryGetValue(capability, out id))
                {
                    chained = ability;
                    return true;
                }
            }

        }

        private void Commit(Entity parent, Capability capability, Definition definition)
        {
            var ability = World.Entity.Create(definition, parent);
            release.AddRange(cancel);
            ProcessCooldowns(ability);
            Promote(parent, capability);
        }

        private void ProcessCooldowns(Entity ability)
        {
            if (!World.Entity.Has<Cooldowns>(ability))
                return;

            foreach(var entry in World.Entity.Cooldowns(ability).Entries)
            {
                if (World.Entity.Phase(ability).Index != entry.Phase)
                    continue;

                CreateCooldown(World.Entity.Parent(ability).Entity, entry);
            }
        }

        private void CreateCooldown(Entity parent, CooldownEntry entry)
        {
            var cooldown = new Definition()
            {
                Id              = $"{entry.Ability} Cooldown",
                Cooldown        = new Cooldown(),
                Duration        = new Duration()        { Length  = entry.Length },
                CooldownTarget  = new CooldownTarget()  { Ability = entry.Ability },
            };

            World.Entity.Create(cooldown, parent);
        }

        private void Promote(Entity entity, Capability capability)
        {
            var command = World.Entity.Command(entity);

            if (!command.Buffer.Remove(capability, out var press))
                return;

            command.Active[capability] = press;
        }

        // Release
        private void ReleaseAbilities()
        {
            foreach (var ability in release)
            {
                World.Entity.Release(ability);
            }

            release.Clear();
        }

        // Advance resolvers
        private bool SustainBroken(Entity ability)
        {
            if (!World.Entity.Has<Sustain>(ability))
                return false;

            var index  = World.Entity.Phase(ability).Index;
            var active = World.Entity.Command(World.Entity.Parent(ability).Entity).Active;

            foreach (var entry in World.Entity.Sustain(ability).Entries)
            {
                if (index < entry.UntilPhase && !active.ContainsKey(entry.Capability))
                    return true;
            }

            return false;
        }

        private bool PhaseSustainReleased(Entity ability)
        {
            var phase   = World.Entity.Phase(ability);   
            var phases  = World.Entity.Phases(ability);
            var parent  = World.Entity.Parent(ability).Entity;
            var active  = World.Entity.Command(parent).Active; 

            return phases.Entry[phase.Index].Until.Any(capability => !active.ContainsKey(capability));
        }

        private bool PhaseDurationElapsed(Entity ability)
        {
            var phase   = World.Entity.Phase(ability);   
            var phases  = World.Entity.Phases(ability);

            return phase.Elapsed >= phases.Entry[phase.Index].Length;
        }

        private bool Finished(Entity ability)
        {
            return World.Entity.Phase(ability).Index >= World.Entity.Phases(ability).Entry.Count;
        }

        // Process Resolvers
        private bool CanResolve(Entity parent, Capability capability, Definition definition)
        {
            var command = World.Entity.Command(parent);

            return ((Activation)definition.Activation).From == Activation.Trigger.Active
                ? command.Active.ContainsKey(capability)
                : command.Buffer.ContainsKey(capability);
        }

        private bool CanActivate(Entity parent, Definition definition)
        {
            if (HasCooldown(parent, definition))
                return false;

            if (!HasSustainTriggers(parent, definition))
                return false;

            return true;
        }

        private bool HasCooldown(Entity parent, Definition ability)
        {
            if (!World.Entity.Has<Child>(parent))
                return false;

            foreach (var child in World.Entity.Child(parent).Entities)
            {
                if (!World.Entity.Has<Cooldown>(child))
                    continue;

                if (World.Entity.CooldownTarget(child).Ability == ability.Id)
                    return true;
            }

            return false;
        }

        private bool HasSustainTriggers(Entity parent, Definition definition)
        {
            if (definition.Sustain is not Sustain sustain)
                return true;

            var command = World.Entity.Command(parent);

            foreach (var entry in sustain.Entries)
            {
                if (!command.Active.ContainsKey(entry.Capability) && !command.Buffer.ContainsKey(entry.Capability))
                    return false;
            }

            return true;
        }

        private bool Validate(Entity parent, Definition definition)
        {
            var kind = ((Activation)definition.Activation).Kind;

            cancel.Clear();

            foreach (var ability in World.Query(Mask<Components, Controls, Phase, Parent>.Key))
            {
                if (World.Entity.Parent(ability).Entity != parent)
                    continue;

                if (release.Contains(ability))
                    continue;

                var result = ControlResult(ability, kind);

                if (result == Controls.Result.Deny)
                    return false;

                if (result == Controls.Result.Cancel)
                    cancel.Add(ability);
            }

            return true;
        }

        private Controls.Result ControlResult(Entity ability, AbilityTag kind)
        {
            var phase = World.Entity.Phase(ability);

            foreach (var entry in World.Entity.Controls(ability).Entries)
            {
                if (entry.Kind != kind || entry.Phase != phase.Index || phase.Elapsed < entry.After)
                    continue;

                return entry.Result;
            }

            return kind == AbilityTag.Instant ? Controls.Result.Coexist : Controls.Result.Deny;
        }

        static AbilitySystem() => Log<AbilitySystem>.Level(Diagnostic.Log.Level.Debug);
    }
}
