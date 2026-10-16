using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;
using System.Linq;
using System.Collections.Generic;
using Game.Common.Events;



namespace Game.Service
{

    public class EffectSystem
    {
        static EffectSystem() => Log<EffectSystem>.Level(Diagnostic.Log.Level.Debug);
    }
    
    public class Dev : RegisteredService, IWorld, IData, IAsset, IInitialize, IRealBase, IRealHalf, IRealStep, IGameBase
    {

        private readonly Data Data;
        private readonly Assets Asset;
        private readonly World World;

        private readonly EffectSystem effect = new();

        private Entity hero;
        private Entity dummy;

        private Entity target;
        private Entity focus;
        private bool   armed;

        public Dev(World world, Data data, Assets asset)
        {
            Data    = data;
            Asset   = asset;
            World   = world;
        }

        public void Initialize()
        {
            hero    = World.Entity.Create(Asset.Get("Hero"), new());
            dummy   = World.Entity.Create(Asset.Get("Dummy"), new() { Position = new() { x = 4, y = 4, z = 0 }});
        }
    
        void IRealBase.Tick() 
        {

        }

        void IGameBase.Tick() 
        {
            Debug();
        }

        void IRealHalf.Tick() 
        {

        }

        void IRealStep.Tick() 
        {

        }

        private void LockOnTest()
        {
            if (Watch.Tick.Game % 600 != 300)
                return;

            var target  = World.Entity.Create(Asset.Get("Dummy"), new() { Position = new() { x = -4, y = -2, z = 0 }});
            var focus   = World.Entity.Create(new Definition { Id = "TestLockOn", Focus = new Focus(), Duration = new Duration { Length = 180 }, Highlight = new Highlight { Takeover = 0.8f, Center = 1f, In = 60, Out = 90, Stickiness = 45, Limit = 7f }}, hero);

            World.Entity.Component.Add(focus,  new CameraFocus { Entity = target });
            World.Entity.Component.Add(target, new Bound { Entity = focus });
        }

        private void ShakeTest()
        {
            var tick = Watch.Tick.Game % 600;

            if (tick == 30)
                Event.Send(new CueCamera { Cue = new Cue { Id = "TestShake", In = 30, Out = 30, Sustained = true, Shake = new Shake { Amplitude = 0.03f, Frequency = 6f } } });

            if (tick == 90)
                Event.Send(new CueCamera { Cue = new Cue { Id = "TestShake", In = 90, Out = 30, Sustained = true, Shake = new Shake { Amplitude = 0.2f, Frequency = 30f } } });

            if (tick == 240)
                Event.Send(new StopCameraCue { Id = "TestShake" });
        }

        private void Debug()
        {
            Log<ResolveSystem>.Debug("Direction.X", () => $"{World.Entity.Intent(hero).Direction.x}");
            Log<ResolveSystem>.Debug("Direction.Y", () => $"{World.Entity.Intent(hero).Direction.y}");

            Log<CommandSystem>.Debug($"Command.Active", () => World.Entity.Command(hero).Active.Count > 0 ? string.Join(", ", World.Entity.Command(hero).Active.Values.Select(comp => comp.Capability)) : "");
            Log<CommandSystem>.Debug($"Command.Buffer", () => World.Entity.Command(hero).Buffer.Count > 0 ? string.Join(", ", World.Entity.Command(hero).Buffer.Values.Select(comp => comp.Capability)) : "");

            foreach (var ability in World.Entity.Children<Ability>(hero))
            {
                if (World.Entity.Parent(ability).Entity != hero)
                    continue;

                Log<AbilitySystem>.Debug($"Ability.Phase.{World.Entity.Identity(ability).Id}", () => string.Join(", ", World.Entity.Phases(ability).Entry[World.Entity.Phase(ability).Index].State));
            }

            foreach (var effect in World.Entity.Children<Effect>(hero))
            {
                Log<EffectSystem>.Debug($"Effect.Elapsed.{World.Entity.Identity(effect).Id}.{World.Entity.Meta(effect).Created}", () => World.Entity.Duration(effect).Remaining);
            }


            StatResolver.Each(ref World.Entity.Modify.Stats(hero), (string name, ref Stat stat) =>
            {
                var row = stat;
                Log<StatSystem>.Debug($"Stat.{name}", () => StatResolver.Describe(row));
            });
        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
