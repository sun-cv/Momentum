using Game.Diagnostic;
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



