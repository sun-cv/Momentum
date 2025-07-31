using System;
using System.Collections.Generic;
using Momentum.Markers;
using Momentum.State;


namespace Momentum.Actor.Hero
{

    public class HeroStateMachine : StateMachine, IStateMachineController
    {

        private Hero                hero;
        private HeroContext         context;

        private IdleState           idle;
        private SprintState         sprint;
        private DashState           dash;
        private BasicAttackState    basicAttack;
        private ShieldBlockState    shieldBlock;

        private Dictionary<Type, StatusFlag> states   = new();

        public void Initialize(Hero hero)
        {
            this.hero       = hero;
            this.context    = hero.context;

            InstantiateStates();
            RegisterCommandStates();
            CreateDefaultTransitions();

            CanTransition.Set();
            SetState(idle);
        }

        void InstantiateStates()
        {
            idle            = new(hero);
            sprint          = new(hero);
            dash            = new(hero);
            basicAttack     = new(hero);
            shieldBlock     = new(hero);

            states.Add(idle.GetType(),        context.state.idle);
            states.Add(sprint.GetType(),      context.state.sprint);
            states.Add(dash.GetType(),        context.state.dash);
            states.Add(basicAttack.GetType(), context.state.basicAttack);
            states.Add(shieldBlock.GetType(), context.state.shieldBlock);
        }

        void RegisterCommandStates()
        {
            Add(dash);
            Add(basicAttack);
            Add(shieldBlock);
        }

        void CreateDefaultTransitions()
        {
            Any<IdleState>  (idle,   () => !hero.context.condition.disabled && context.movement.idle);
            Any<SprintState>(sprint, () => !hero.context.condition.disabled && context.movement.locomotion);
        }

    }
}


        // private void At<T>(IState from, IState to, Func<bool> condition)    => AddTransition(from, to, condition);




