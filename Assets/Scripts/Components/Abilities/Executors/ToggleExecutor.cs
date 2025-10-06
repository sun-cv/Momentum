

namespace Momentum
{

    public class ToggleExecutor : AbilityExecutor
    {
        public ToggleExecutor(AbilityInstance instance) : base(instance) {}


        protected override void OnTick()
        {
            if (phase == AbilityPhase.Active)
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