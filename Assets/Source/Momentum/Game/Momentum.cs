using Game.Core;
using Game.Data;
using Game.Realm;



namespace Game
{
    
    public class Momentum
    {
        private readonly Engine     engine;
        private readonly World      world;
        private readonly Definition definition; 

        public Momentum()
        {
            engine  = new();
            world   = new();

        }

        public void Shutdown()
        {
            engine  .Shutdown();
            world   .Shutdown();
        }

        public Core.Engine Engine => engine;
    }
}

