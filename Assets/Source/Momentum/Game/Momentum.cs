using Game.Core;
using Game.Common;
using Game.Content;
using Game.Realm;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;



namespace Game
{
    
    public class Momentum
    {
        private readonly Engine     engine;
        private readonly World      world;
        private readonly Registry   registry; 
        private readonly Data       data ;
        private readonly Assets     asset;

        public Momentum()
        {
            engine      = new();
            world       = new();
            registry    = new();
            data        = new(registry);
            asset       = new(registry);

        }
        public List<AsyncOperationHandle> Boot()
        {
            asset.Load.Prefab.Load(new() {"Prefab"});
            return data.Boot();
        }

        public void Initialize()
        {
            engine.Scanner.Register(world, data, asset);
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

