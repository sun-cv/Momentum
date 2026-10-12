using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;
using System.Linq;
using System.Collections.Generic;



namespace Game.Service
{

    public class Dev : RegisteredService, IWorld, IData, IAsset, IInitialize, IRealBase, IRealHalf, IRealStep, IGameBase
    {

        private readonly Data Data;
        private readonly Assets Asset;
        private readonly World World;

        private Entity hero;
        private List<Entity> children = new();

        public Dev(World world, Data data, Assets asset)
        {
            Data    = data;
            Asset   = asset;
            World   = world;
        }

        public void Initialize()
        {
            hero = World.Entity.Create(Asset.Get("Hero"), new());

            World.Entity.Create(Asset.Get("Dummy"), new() { Position = new() { x = 4, y = 4, z = 0 }});
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

        private void Debug()
        {
            Log<Dev>.Debug("Direction.X", () => $"{World.Entity.Intent(hero).Direction.x}");
            Log<Dev>.Debug("Direction.Y", () => $"{World.Entity.Intent(hero).Direction.y}");

            Log<Dev>.Debug($"Command.Active", () => World.Entity.Command(hero).Active.Count > 0 ? string.Join(", ", World.Entity.Command(hero).Active.Values.Select(comp => comp.Capability)) : "");
            Log<Dev>.Debug($"Command.Buffer", () => World.Entity.Command(hero).Buffer.Count > 0 ? string.Join(", ", World.Entity.Command(hero).Buffer.Values.Select(comp => comp.Capability)) : "");

            if (World.Entity.Has<Child>(hero))
            {
                children.Clear();
                children.AddRange(World.Entity.Child(hero).Entities.Where(entity => World.Entity.Has<Ability>(entity)));
            }

            foreach(var ability in children)
            {
                Log<Dev>.Debug($"Ability.Phase.{World.Entity.Identity(ability).Id}", () => children.Count > 0 ? string.Join(", ", World.Entity.Phases(ability).Entry[World.Entity.Phase(ability).Index].State) : "");
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
