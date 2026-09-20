using Game.Realm;
using Game.Common;
using Game.Diagnostic;



namespace Game.Service
{
    
    public class MovementSystem : RegisteredService, IWorld, IGameBase
    {

        private readonly World World;

        public MovementSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            ProcessMovement();
        }
        
        private void ProcessMovement()
        {
            ResolveMovementEffects();
            
            // ProcessEntityMovement();
        }

        private void ResolveMovementEffects()
        {
            
        }
        

        static MovementSystem() => Log<MovementSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}
