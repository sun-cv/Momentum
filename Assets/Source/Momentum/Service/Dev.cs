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

        private Entity entity;
        private List<Entity> children = new();

        public Dev(World world, Data data, Assets asset)
        {
            Data    = data;
            Asset   = asset;
            World   = world;
        }

        public void Initialize()
        {
            entity = World.Entity.Create(Asset.Get("Hero"), new());

            World.Entity.Create(Asset.Get("Dummy"), new() { Position = new() { x = 4, y = 4, z = 0 }});
        }
    
        void IRealBase.Tick() 
        {
            var commands  = World.Entity.Command(entity);
           
            if (World.Entity.Has<Child>(entity))
            {
                children.Clear();
                children.AddRange(World.Entity.Child(entity).Entities.Where(entity => World.Entity.Has<Ability>(entity)));
            }

            Log<Dev>.Debug($"Command.Active", () => commands.Active.Count > 0 ? string.Join(", ", commands.Active.Values.Select(comp => comp.Capability)) : "");
            Log<Dev>.Debug($"Command.Buffer", () => commands.Buffer.Count > 0 ? string.Join(", ", commands.Buffer.Values.Select(comp => comp.Capability)) : "");

            Log<Dev>.Debug($"Ability.Active", () => children.Count > 0 ? string.Join(", ", children.Select(entity => World.Entity.Identity(entity).Id)) : "");

            foreach(var ability in children)
            {
                Log<Dev>.Debug($"Ability.Phase", () => children.Count > 0 ? string.Join(", ", World.Entity.Phases(ability).Entry[World.Entity.Phase(ability).Index].State) : "");
            }
        }

        void IGameBase.Tick() 
        {

        }

        void IRealHalf.Tick() 
        {

        }

        void IRealStep.Tick() 
        {
            var direction = World.Entity.Intent(entity).Direction;
            var commands  = World.Entity.Command(entity);

            Log<Dev>.Debug("Direction.X", () => $"{direction.x}");
            Log<Dev>.Debug("Direction.Y", () => $"{direction.y}");
        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
