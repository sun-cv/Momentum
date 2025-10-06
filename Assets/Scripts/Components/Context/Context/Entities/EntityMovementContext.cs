

using UnityEngine;

namespace Momentum
{

    public class EntityMovementContext : ContextRoot
    {
        public DynamicFlag idle;
        public DynamicFlag sprint;

        public MovementIntent intent;

        public Vector2   lastDirection;
        public Principal principalDirection;

        public EntityMovementContext()
        {
            intent  = new();
            idle    = new(() => intent.direction == Vector2.zero);
            sprint  = new(() => intent.direction != Vector2.zero);
        }
    }


}