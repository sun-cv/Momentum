

namespace Momentum
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

        public void Initialize(Hero hero)
        {
            this.hero       = hero;
            this.context    = hero.context;

            InstantiateStates();
            CreateDefaultTransitions();

            SetState(idle);
        }

        void InstantiateStates()
        {
            idle            = new(hero);
            sprint          = new(hero);
            dash            = new(hero);
            basicAttack     = new(hero);
            shieldBlock     = new(hero);

            Add(idle,        context.state.idle);
            Add(sprint,      context.state.sprint);
            Add(dash,        context.state.dash);
            Add(basicAttack, context.state.basicAttack);
            Add(shieldBlock, context.state.shieldBlock);
        }

        void CreateDefaultTransitions()
        {
            Any<IdleState>  (idle,   () => !hero.context.condition.disabled && context.movement.idle);
            Any<SprintState>(sprint, () => !hero.context.condition.disabled && context.movement.locomotion);
        }

    }
}




