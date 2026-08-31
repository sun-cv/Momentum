


namespace Game
{
    
    public class Momentum
    {
        private Engine.Core     engine;
        private World.Context   world;
        
        public Momentum()
        {
            engine  = new();
            world   = new();

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
 
namespace Game
{

    public class Orchestrator
    {

    }
}

namespace Game.World
{

    class Context
    {
        public Context()
        {

        }

        public void Shutdown()
        {
        }
    }
}


