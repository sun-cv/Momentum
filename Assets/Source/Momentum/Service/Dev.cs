using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;
using System.Linq;



namespace Game.Service
{

    public class Dev : RegisteredService, IWorld, IData, IAsset, IInitialize, IRealBase, IRealHalf, IRealStep, IGameBase
    {

        private readonly Data Data;
        private readonly Assets Asset;
        private readonly World World;

        private readonly Entity entity;

        public Dev(World world, Data data, Assets asset)
        {
            Data    = data;
            Asset   = asset;
            World   = world;
        }

        public void Initialize()
        {
            World.Entity.Create(Asset.Get("Hero"), new());
            World.Entity.CanMove(entity);
        }
    
        void IRealBase.Tick() 
        {

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
       
            Log<Dev>.Debug($"Command.Active", () => commands.Buffer.Count > 0 ? string.Join(", ", commands.Buffer.Values.Select(comp => comp.Capability)) : "");

        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
