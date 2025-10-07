

namespace Momentum.Abilities
{

    public class ToggleExecutor : AbilityExecutor
    {
        public ToggleExecutor(Instance instance) : base(instance) {}


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