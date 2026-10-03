using System;

using Game.Realm;
using Game.Common;
using Game.Content;
using System.Collections.Generic;
using System.Linq;
using Game.Diagnostic;




namespace Game.Service
{

    public class AbilitySystem : RegisteredService, IWorld, IData, IGameBase
    {
        private readonly Data Data;
        private readonly World World;

        private readonly List<Entity> release = new();

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

        private void AdvanceAbilities()
        {
            foreach (var entity in World.Query(Mask<Components, Ability>.Key))
            {
                AdvanceAbility(entity);
            }
        }

        private void AdvanceAbility(Entity ability)
        {
            if (ShouldTerminate(ability))
            {
                // Deactivate(ability);
                // Make inert, check control window, add duration component, remove render etc.
                return; 
            }

            if (PhaseSustainReleased(ability) || PhaseDurationElapsed(ability))
            {
                AdvancePhase(ability); 
            }
            
            Advance(ability);
        }

        private void AdvancePhase(Entity ability)
        {
            World.Entity.Modify.Phase(ability).Index++;
            World.Entity.Modify.Phase(ability).Elapsed  = 0;
        }

        private void Advance(Entity ability)
        {
            if (PhaseDurationElapsed(ability))
            {
                Advance(ability);
                return;
            }
        }

        private void ReleaseAbilities()
        {
            foreach (var ability in release)
            {
                World.Entity.Release(ability);
            }

            release.Clear();
        }

        // ControlWindow requires implementation - designates control window open on ability
        private void ProcessAbilities()
        {
            foreach (var ability in World.Query(Mask<Components, Ability, Chains, ControlWindow>.Key))
            {
                ProcessLoadout(World.Entity.Parent(ability).Entity, World.Entity.Chains(ability).Abilities);
            }

            foreach (var entity in World.Query(Mask<Components, Commands>.Key))
            {
                ProcessLoadout(entity, World.Entity.Loadout(entity).Abilities);
            }
        }

        private void ProcessLoadout(Entity parent, Dictionary<Capability, string> loadout)
        {
            foreach (var (capability, ability) in loadout)
            {
                ProcessAbility(parent, capability, ability);
            }
        }

        private void ProcessAbility(Entity parent, Capability capability, string name)
        {
            Definition ability = Data.Definition(name);
                
            if (!CanResolve(parent, capability, ability))
                return;

            if (!CanActivate(parent, ability))
                return;

            if (!Validate(parent, ability))
                return;

            Create(parent, ability);
        }

        private bool CanResolve(Entity parent, Capability capability, Definition ability)
        {
            if (ability.Activation is { } activation && activation.From == Activation.Trigger.Active != World.Entity.Command(parent).Active.ContainsKey(capability))
                return false

            if (!World.Entity.Command(parent).Buffer.ContainsKey(capability))
                return false;

            return true;
        }

        private bool CanActivate(Entity parent, Definition ability)
        {
            if (HasCooldown(parent, ability))
                return false;

            if (!HasSustainTriggers(parent, ability))
                return false;

            return true;
        }

        private bool HasSustainTriggers(Entity parent, Definition ability)
        {
            if (ability.Activation is not Activation activation || ability.Sustain is not Sustain sustain)
                return false;

            if (activation.From == Activation.Trigger.Active )
                return false;
            
            foreach(var entry in sustain.Entries)
            {
                if (!World.Entity.Command(parent).Active.ContainsKey(entry.Capability))
                {
                    return false;
                }
            }
            return true;
        }

        private void Create(Entity parent, string ability)
        {
            World.Entity.Create(Data.Definition(ability), parent);
        }

        private void PromoteCapability(Entity entity, List<Capability> capabilities)
        {
            var active = World.Entity.Command(entity).Active;
            var buffer = World.Entity.Command(entity).Buffer;

            foreach(var capability in capabilities)
            {
                if (active.Keys.Contains(capability))
                    continue;

                active[capability] = buffer[capability];

                buffer.Remove(capability);
            }
        }

        private bool PhaseSustainReleased(Entity ability)
        {
            var phase   = World.Entity.Phase(ability);   
            var phases  = World.Entity.Phases(ability);
            var parent  = World.Entity.Parent(ability).Entity;
            var active  = World.Entity.Command(parent).Active; 

            Log<AbilitySystem>.Debug(phase.Index);
            Log<AbilitySystem>.Debug(phase.Elapsed);
            Log<AbilitySystem>.Debug(phases.Entry[phase.Index].Until.Any(capability => !active.ContainsKey(capability)));

            return phases.Entry[phase.Index].Until.Any(capability => !active.ContainsKey(capability));
        }

        private bool PhaseDurationElapsed(Entity ability)
        {
            var phase   = World.Entity.Phase(ability);   
            var phases  = World.Entity.Phases(ability);

            return phase.Elapsed >= phases.Entry[phase.Index].Length;
        }

        private bool ShouldTerminate(Entity ability)
        {
            var phase   = World.Entity.Phase(ability);   
            var phases  = World.Entity.Phases(ability);
            var parent  = World.Entity.Parent(ability).Entity;
            var active  = World.Entity.Command(parent).Active; 
            var sustain = World.Entity.Sustain(ability).Entries;

            foreach(var entry in sustain)
            {
                if (phase.Index <= entry.UntilPhase && !active.ContainsKey(entry.Capability))
                {
                    return true;
                }
            }

            return phase.Index >= phases.Entry.Count - 1;
        }

        private bool HasCooldown(Entity parent, Definition ability)
        {
            foreach (var child in World.Entity.Child(parent).Entities)
            {
                if (!World.Entity.Has<Cooldown>(child))
                    continue;

                if (World.Entity.CooldownTarget(child).Ability == ability.Id)
                    return true;
            }

            return false;
        }


        static AbilitySystem() => Log<AbilitySystem>.Level(Diagnostic.Log.Level.Debug);
    }
}
