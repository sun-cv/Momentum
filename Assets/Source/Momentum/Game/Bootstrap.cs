using System.Collections;
using UnityEngine;



namespace Game
{

    class Bootstrap : MonoBehaviour
    {

        private Momentum momentum;

        public void Awake()
        {
            momentum = new();
            enabled  = false;
            StartCoroutine(Boot());
        }

        private IEnumerator Boot()
        {
            foreach (var handle in momentum.Boot())
                yield return handle;

            momentum.Initialize();

            enabled = true;
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



