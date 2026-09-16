using Game.Core;
using Game.Content;
using Game.Realm;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;



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
        public List<AsyncOperationHandle> Boot()
        {
            return data.Boot();
        }

        public void Initialize()
        {
            engine.Scanner.Register(world, data);
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

