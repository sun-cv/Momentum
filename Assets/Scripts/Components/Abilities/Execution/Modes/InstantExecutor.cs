

namespace Momentum.Abilities
{

    public class InstantExecutor : Executor
    {
        public InstantExecutor(ExecutionManager manager, Instance instance) : base(manager,instance) {}

        protected override void OnTick()
        {
            if (AllEffectPhase(EffectPhase.Active))
            {
                Active();
                Execute();
            }

            if (AllEffectPhase(EffectPhase.Completed))
            {
                Complete();
                Deactivate();
            }
        }


    }
}