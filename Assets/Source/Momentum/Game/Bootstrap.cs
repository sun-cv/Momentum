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
        }

        public void FixedUpdate()
        {
            momentum.Engine         .Tick();
            momentum.Engine.Clock   .Tick();
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



