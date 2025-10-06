

using System;

namespace Momentum
{

    public class IntentCast : Intent
    {
        public HeroIntent Hero => this as HeroIntent ?? throw new InvalidOperationException("This intent is not a HeroIntent");
    }

}