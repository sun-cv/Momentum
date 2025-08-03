
using UnityEngine;


namespace Momentum
{

    // REWORK REQUIRED - Discovered issues with camera micro stutters due to bad control handling when forcing movement through state.
        // Reworked -- testing required to confirm fix;
    public enum MovementMode
    {
        Disabled,
        Dynamic,
        Kinematic,
        Impulse,
    }

    public enum MovementIntent
    {
        Idle,
        Move,
        Dash,
        Attack,
    }

    public enum MovementCondition
    {
        Disabled,
        Slowed,
        Stunned,
        KnockedBack,
        Staggered,
    }

    public class MovementEngine
    {   
        HeroContext                     context;

        Rigidbody2D                     body;
        MovementAttribute               attribute;

        HeroContext.Movement            movement;
        HeroContext.Condition           condition;

        Vector2                         currentDirection;
        bool                            clearMomentum;

        public void Initialize(HeroContext context)
        {
            this.context    = context;
            this.body       = context.body;
            this.attribute  = context.attributes.movement;

            this.movement   = context.movement;
            this.condition  = context.condition;

            body.freezeRotation = true;
            body.gravityScale   = 0f;
        }

        public void Tick()
        {
            // noop
        }

        public void TickFixed()
        {
            HandleMovementMode();
        }

        void HandleMovementMode()
        {
            switch (movement.mode)
            {
                case MovementMode.Disabled:
                    HandleDisabled();
                break;

                case MovementMode.Kinematic:
                    HandleKinematic();
                break;

                case MovementMode.Dynamic:
                    HandleDynamic();
                break;

                case MovementMode.Impulse:
                    HandleImpulse();
                break;
            }

        }

        void HandleDisabled()
        {
            ApplyFriction();
            ApplyFinalVelocity();
            return;
        }

        void HandleKinematic()
        {
            CalculatePosition();
            ApplyPosition();
        }

        void CalculatePosition()
        {
            movement.position = Vector2.Lerp(movement.startPosition, movement.targetPosition, movement.progress);
        }

        void ApplyPosition()
        {
            body.MovePosition(movement.position);
        }

        void HandleDynamic()
        {
            SetDirection();
            SetVelocity();

            CalculateMomentum();
            CalculateMovementVelocity();

            ApplyFinalVelocity();
        }

        void SetDirection()
        {
            if (currentDirection != movement.direction)
            {
                clearMomentum       = movement.direction != Vector2.zero;
                currentDirection    = movement.direction;
            }
        }

        void SetVelocity()
        {
            movement.velocity       = currentDirection * attribute.speed ;
            movement.baseVelocity   = movement.velocity;
        }

        void CalculateMomentum()
        {
            if (clearMomentum)
            {
                movement.momentum   = Vector2.zero;
                clearMomentum       = false;
            }

            switch (movement.intent)
            {
                case MovementIntent.Idle:
                    ApplyFriction();
                break;

                case MovementIntent.Move:
                    ApplyMomentum();
                break;
            }
        }

        void ApplyFriction()
        {
            movement.momentum = Vector2.MoveTowards(movement.momentum, Vector2.zero, attribute.groundFriction * Time.fixedDeltaTime);
        }

        void ApplyMomentum()
        {
            movement.momentum += attribute.groundControlRate * Time.fixedDeltaTime * movement.baseVelocity;
            movement.momentum = Vector2.ClampMagnitude(movement.momentum, attribute.speed);
        }

        void CalculateMovementVelocity()
        {
            movement.finalVelocity  = movement.velocity + movement.momentum;
        }

        void ApplyFinalVelocity()
        {
            body.linearVelocity = movement.finalVelocity;
        }
    
        void HandleImpulse()
        {
            ApplyImpulse();
            HandlePreserveMomentum();
        }

        public void ApplyImpulse()
        {
            if (movement.impulseRequest.Consume())
            {
                movement.momentum = movement.impulseDirection.normalized * movement.force;
            }
        }

        void HandlePreserveMomentum()
        {
            ApplyFriction();
            movement.velocity = Vector2.zero; 
            CalculateMovementVelocity();
            ApplyFinalVelocity();
        }
    }
}
