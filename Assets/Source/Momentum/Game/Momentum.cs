


namespace Game
{
    
    public class Momentum
    {
        private Engine.Core     engine;
        private World.Context   world;
        
        public void Initialize()
        {
            engine  = new();
            world   = new();

            engine  .Initialize();
            world   .Initialize();
        }
    
        public void Shutdown()
        {
            engine  .Shutdown();
            world   .Shutdown();
        }

        public Engine.Core Engine => engine;
    }
}
 

namespace Game.World
{

    class Context
    {

        public void Initialize()
        {
        }
        public void Shutdown()
        {
        }
    }
}


