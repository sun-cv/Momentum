

namespace Game
{
    
    public class Momentum
    {
        private readonly Engine.Core    engine;
        private readonly World.Context  world;
        
        public Momentum()
        {
            engine  = new();
            world   = new();

            Service.Service.Initialize();
            engine.Initialize();
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


