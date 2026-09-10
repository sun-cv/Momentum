using Game.Core;
using Game.Content;
using Game.Realm;



namespace Game
{
    
    public class Momentum
    {
        private readonly Engine engine;
        private readonly World  world;
        private readonly Data   data ; 

        public Momentum()
        {
            engine  = new();
            world   = new();
            data    = new();
        }

        public void Shutdown()
        {
            engine  .Shutdown();
            world   .Shutdown();
            data    .Shutdown();
        }

        public Core.Engine Engine => engine;
    }
}

