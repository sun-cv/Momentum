

namespace Momentum.Abilities
{

    public class InstantExecutor : AbilityExecutor
    {
        public InstantExecutor(Instance instance) : base(instance) {}

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