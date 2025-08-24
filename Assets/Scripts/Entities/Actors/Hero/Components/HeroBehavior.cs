using System.Linq;
using Momentum.HSM.Hero.Combat;
using Momentum.HSM.Hero.Movement;
using UnityEngine;


namespace Momentum
{


    public class HeroBehavior : BehaviorComponent 
    {
        private StateMachine movement;
        private StateMachine combat;
        
        private State rootMovement;
        private State rootCombat;

        private string lastPathmovement;
        private string lastPathCombat;


        public void Awake()
        {

            Debug.Log("Awake");

            rootMovement = new RootMovement();
            rootCombat   = new RootCombat();

            var movementBuilder = new StateMachineBuilder(rootMovement);
            var combatBuilder   = new StateMachineBuilder(rootCombat);

            movement = movementBuilder.Build();
            combat   = combatBuilder.Build();

            movement.Start();
            combat  .Start();

        }

        public void Update()
        {

            movement.Tick(Time.deltaTime);
            combat  .Tick(Time.deltaTime);

            DebugLogState();
        }


        void DebugLogState()
        {
            var pathmovement = StatePath(movement.Root.Leaf());
            var pathcombat   = StatePath(combat.Root.Leaf());

            if (pathmovement != lastPathmovement) {
                Logwin.Log("Movement:", pathmovement, "State Machine");
                lastPathmovement = pathmovement;
            }

            if (pathcombat != lastPathCombat) {
                Logwin.Log("Combat:", pathcombat, "State Machine");
                lastPathCombat = pathcombat;
            }
        }

        static string StatePath(State s)
        {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }

    }





}