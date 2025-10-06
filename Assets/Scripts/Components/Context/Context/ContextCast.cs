using System;


namespace Momentum
{


    public class ContextCast : ContextRoot
    {
        public HeroContext Hero => this as HeroContext ?? throw new InvalidOperationException("This context is not a HeroContext");

    }

    public class ContextMovementCast : ContextRoot
    {
        // public HeroContext Hero => this as HeroMovementContext ?? throw new InvalidOperationException("This context is not a HeroContext");
    }
}
