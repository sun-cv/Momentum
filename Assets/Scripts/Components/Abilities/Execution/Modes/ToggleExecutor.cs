

namespace Momentum.Abilities
{

    public class ToggleExecutor : Executor
    {
        public ToggleExecutor(ExecutionManager manager, Instance instance) : base(manager,instance) {}


        protected override void OnTick()
        {
            if (phase == Phase.Active)
            {
                Complete();
            }
            else
            {
                Activate();
            }

            if (AllEffectPhase(EffectPhase.Active))
            {
                Active();
            }
        }
    }


}