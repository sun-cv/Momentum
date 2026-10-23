using System.Linq;

using Game.Realm;
using Game.Service;
using Game.Common;
using Game.Diagnostic;

namespace Game.Development
{

    public static partial class Log
    {
        public static class Actor
        {
            public static void Movement(World World, Entity entity)
            {
                Log<ResolveSystem>.Debug("Direction.X", () => $"{World.Entity.Intent(entity).Direction.x}");
                Log<ResolveSystem>.Debug("Direction.Y", () => $"{World.Entity.Intent(entity).Direction.y}");
            }

            public static void Command(World World, Entity entity)
            {
                Log<CommandSystem>.Debug($"Command.Active", () => World.Entity.Command(entity).Active.Count > 0 ? string.Join(", ", World.Entity.Command(entity).Active.Values.Select(comp => comp.Capability)) : "");
                Log<CommandSystem>.Debug($"Command.Buffer", () => World.Entity.Command(entity).Buffer.Count > 0 ? string.Join(", ", World.Entity.Command(entity).Buffer.Values.Select(comp => comp.Capability)) : "");
            }

            public static void Ability(World World, Entity entity)
            {
                foreach (var ability in World.Entity.Children<Ability>(entity))
                {
                    if (World.Entity.Parent(ability).Entity != entity)
                        continue;

                    Log<AbilitySystem>.Debug($"Ability.Phase.{World.Entity.Identity(ability).Id}", () => string.Join(", ", World.Entity.Phases(ability).Entry[World.Entity.Phase(ability).Index].State));
                }
            }

            public static void Effects(World World, Entity entity)
            {
                foreach (var effect in World.Entity.Children<Effect>(entity))
                {
                    Log<Dev>.Debug($"Effect.Elapsed.{World.Entity.Identity(effect).Id}.{World.Entity.Meta(effect).Created}", () => World.Entity.Duration(effect).Remaining);
                }
            }

            public static void Stats(World World, Entity entity)
            {
                Log<Dev>.Debug($"Stat.Health",      () => World.Entity.Stats(entity).Health.Final);
                Log<Dev>.Debug($"Stat.Armor",       () => World.Entity.Stats(entity).Armor.Final);
                Log<Dev>.Debug($"Stat.Energy",      () => World.Entity.Stats(entity).Energy.Final);
                Log<Dev>.Debug($"Stat.Damage",      () => World.Entity.Stats(entity).Damage.Final);
                Log<Dev>.Debug($"Stat.Speed",       () => World.Entity.Stats(entity).Speed.Final);
                Log<Dev>.Debug($"Stat.CritChange",  () => World.Entity.Stats(entity).CritChance.Final);
                Log<Dev>.Debug($"Stat.CritDamage",  () => World.Entity.Stats(entity).CritDamage.Final);
            }

            public static void Resources(World World, Entity entity)
            {
                Log<Dev>.Debug($"Resource.Health",  () => World.Entity.Health(entity).Current);
                Log<Dev>.Debug($"Resource.Armor",   () => World.Entity.Armor(entity).Current);
                Log<Dev>.Debug($"Resource.Energy",  () => World.Entity.Energy(entity).Current);
            }
        }
    }
}
