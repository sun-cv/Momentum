using UnityEngine;



namespace Game
{

    class Bootstrap : MonoBehaviour
    {
        
        private Momentum momentum;

        public void Awake()
        {
            momentum = new();
            momentum.Initialize();
        }

        public void FixedUpdate()
        {
            momentum.Engine.Tick();
        }

        public void LateUpdate()
        {
            momentum.Engine.Late();
        }

        public void OnDisable()
        {
            momentum.Shutdown();
        }
    }
}


namespace Game
{
    
    class Momentum
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


namespace Game.Engine
{

    class Core
    {
        private Engine.Clock    clock;
        private Engine.Loop     loop;

        public void Initialize()
        {
            clock   = new();
            loop    = new();

            clock   .Initialize();
            loop    .Initialize();
        }

        public void Tick()
        {

        }        

        public void Late()
        {

        } 

        public void Shutdown()
        {
            clock   .Dispose();
            loop    .Dispose();
        }
    }
}


namespace Game.Engine
{

    class Clock
    {

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}


namespace Game.Engine
{

    class Loop
    {

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
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


