using System.Collections.Generic;
using UnityEngine;


namespace Momentum
{

    public interface IService : ITickAll {};

    public class GameManager : MonoBehaviour
    {

        GameBootstrapper bootstrapper   = new();
        List<IService> services         = new();

        private void Awake()
        {
            bootstrapper.Initialize();
            RegisterServices();
        }

        void RegisterServices()
        {
            GameTickBinding.Register();
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

        private void Register<T>(T system) where T : IService
        {
            if (!services.Contains(system))
            {
                services.Add(system);
            }
        }

        private void Deregister<T>(T system) where T : IService
        {
            if (!services.Contains(system))
            {
                return;
            }
            services.Remove(system);
        }
    }
}