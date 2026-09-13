using Game.Common;
using Game.Content;
using Game.Diagnostic;
using Game.Realm;



namespace Game.Service
{
   
    public class PlayerInputSystem : RegisteredService, IInitialize, IRealBase, IWorld, IData
    {

        private readonly Data Data;
        private readonly World World; 

        public PlayerInputSystem(World world, Data data)
        {
            Data    = data;
            World   = world;
        }

        public void Initialize()
        {

        }

        public void Tick()
        {
            
        }

        static PlayerInputSystem() => Log<PlayerInputSystem>.Level(Diagnostic.Log.Level.Debug);
    }


}


