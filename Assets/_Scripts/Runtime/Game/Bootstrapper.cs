using UnityEngine;


namespace Momentum
{


    public class Bootstrapper
    {

        Hero                        hero;
        EntitySystem                entitySystem;
        InputDriverMono             inputDriverMono;
        CooldownHandler             cooldownHandler;
        ValidatorService            validatorService;


        public void Initialize()
        {
            LocateMonoBehaviors();
            InstantiateSystems();
            RegisterAllSystems();
            InitializeSystems();
            service();
        }

        void LocateMonoBehaviors()
        {
            hero            = Object.FindFirstObjectByType<Hero>();
            inputDriverMono = Object.FindFirstObjectByType<InputDriverMono>();
        }

        void InstantiateSystems()
        {
            entitySystem        = new();
            cooldownHandler     = new();
            validatorService    = new();
        }


        void RegisterAllSystems()
        {
            Registry.Register<IInputDriverMono>(inputDriverMono);
            Registry.Register<IEntitySystem>(entitySystem);
            Registry.Register<IHero>(hero);
            Registry.Register<ICooldownHandler>(cooldownHandler);
            Registry.Register<IValidatorService>(validatorService);
        }

        void InitializeSystems()
        {
            entitySystem.Initialize();
        }

        void service()
        {
            validatorService.Register<HeroContext>(hero.context);
            validatorService.Register<ICooldownHandler>(cooldownHandler);
        }

    }







}