using Game.Common;
using Game.Realm;



namespace Game.Service
{

    public class CombatSystem : RegisteredService, IGameBase, IWorld
    {

        private readonly World World;
        
        public CombatSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {

        }
 
    }
}

