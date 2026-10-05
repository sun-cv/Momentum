using Game.Core;
using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Tooling;
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
        
        private readonly HitboxGizmos gizmos;

        public Momentum()
        {
            engine      = new();
            world       = new();
            registry    = new();
            data        = new(registry);
            asset       = new(registry);

            gizmos      = new(world);

        }
        public List<AsyncOperationHandle> Boot()
        {
            asset.Load.Prefab.Load(new() {"Prefab"});

            var handles = data.Boot();
            handles.Add(asset.Load.Sheet.Load(new() {"Sheet"}));

            return handles;
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

        public void DrawGizmos()
        {
            gizmos.Draw();
        }

        public Core.Engine Engine => engine;
    }
}

