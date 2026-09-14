using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;



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
            id      = World.Entity.Create.Actor(Data.Lookup.Actor("Hero"));
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
            var test = World.Query(Mask<Intent>.Key);
            
            World.Entity.Health(id);
            World.Entity.Modify.Health(id);

            foreach (var entity in test)
            {
                var direction = World.Entity.Intent(entity).Direction;

                Log<Dev>.Debug("Direction.X", () => $"{direction.x}");
                Log<Dev>.Debug("Direction.Y", () => $"{direction.y}");

            }
        }

        static Dev() => Log<Dev>.Level(Diagnostic.Log.Level.Debug);                
    }
}
