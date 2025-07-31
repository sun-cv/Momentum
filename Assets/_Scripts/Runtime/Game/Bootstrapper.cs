using Momentum.Actor.Hero;
using Momentum.Definition;
using Momentum.Input;
using Momentum.Interface;
using UnityEngine;


namespace Momentum
{


    public class Bootstrapper
    {

        Hero                hero;
        EntitySystem        entitySystem;
        InputDriverMono     inputDriverMono;


        public void Initialize()
        {
            LocateMonoBehaviors();
            InstantiateSystems();
            RegisterAllSystems();
            InitializeSystems();
        }

        void LocateMonoBehaviors()
        {
            inputDriverMono = Object.FindFirstObjectByType<InputDriverMono>();
            hero            = Object.FindFirstObjectByType<Hero>();
        }

        void InstantiateSystems()
        {
            entitySystem   = new();
        }


        void RegisterAllSystems()
        {
            Registry.Register<IInputDriverMono>(inputDriverMono);
            Registry.Register<IEntitySystem>(entitySystem);
            Registry.Register<IEntityHero>(hero);
        }

        void InitializeSystems()
        {
            entitySystem.Initialize();
        }

    }







}