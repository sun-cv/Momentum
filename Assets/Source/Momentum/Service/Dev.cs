using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;
using System.Linq;



namespace Game.Service
{

    public class Dev : RegisteredService, IWorld, IData, IInitialize, IRealBase, IRealHalf, IRealStep, IGameBase
    {

        private readonly World World;
        private readonly Data Data;

        private readonly Entity id;
        private readonly Actor actor; 

        public Dev(World world, Data data)
        {
            Data    = data;
            World   = world;
            id      = World.Entity.Create.Actor(Data.Load.Definition<Definition.Actor>("Hero"));
        }

        public void Initialize()
        {
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
            var direction = World.Entity.Intent(id).Direction;
            var commands  = World.Entity.Command(id);

            Log<Dev>.Debug("Direction.X", () => $"{direction.x}");
            Log<Dev>.Debug("Direction.Y", () => $"{direction.y}");
       
            Log<Dev>.Debug($"Command.Active", () => commands.Buffer.Count > 0 ? string.Join(", ", commands.Buffer.Values.Select(comp => comp.Capability)) : "");

        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
