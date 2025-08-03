using System.Collections.Generic;
using UnityEngine;

namespace Momentum
{

    public class GameManager : MonoBehaviour
    {

        Bootstrapper bootstrapper   = new();
        List<ITickAll> services     = new();

        private void Start()
        {
            bootstrapper.Initialize();
            GameTickBinding.Register();

            Register(Registry.Get<IEntitySystem>());
        }


        private void Update()
        {
            EventBus<Tick>.Raise(new Tick());
            
            foreach (var service in services)
            {
                service.Tick();
            }

        }

        private void FixedUpdate()
        {
            EventBus<TickFixed>.Raise(new TickFixed());
            
            foreach (var service in services)
            {
                service.TickFixed();
            }
        }

        private void LateUpdate()
        {
            EventBus<TickLate>.Raise(new TickLate());

            foreach (var service in services)
            {
                service.TickLate();
            }
        }

        private void OnDisable()
        {
            GameTickBinding.Deregister();
            services.Clear();
        }

        private void Register<T>(T system) where T : ITickAll
        {
            if (!services.Contains(system))
            {
                services.Add(system);
            }
        }

        private void Deregister<T>(T system) where T: ITickAll
        {
            if (!services.Contains(system))
            {
                return;
            }
            services.Remove(system);
        }
    }
}