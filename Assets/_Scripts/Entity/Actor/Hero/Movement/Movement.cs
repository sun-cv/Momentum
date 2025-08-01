using UnityEditor.Experimental.GraphView;
using UnityEngine;



namespace Momentum.Actor.Hero
{

    // REWORK REQUIRED - Discovered issues with camera micro stutters due to bad control handling when forcing movement through state.

    public class MovementEngine
    {   
        HeroContext                     context;
    
        Transform                       transform;
        Rigidbody2D                     body;
        MovementAttribute               attribute;
    
        HeroContext.Movement            movement;
        HeroContext.Condition           condition;
    
        Vector2                         currentDirection;
        bool                            clearMomentum;
    
        public void Initialize(HeroContext context)
        {
            this.context    = context;
            transform       = context.transform;
            body            = context.body;
            attribute       = context.attributes.movement;
    
            movement        = context.movement;
            condition       = context.condition;
    
            body.freezeRotation = true;
            body.gravityScale   = 0f;
        }
    
        public void Tick()
        {
            // noop
        }
    
        public void TickFixed()
        {
            if (condition.disabled)
            {
                ApplyFriction();
                ApplyMovementVelocity();
                return;
            }
    
            SetDirection();
    
            SetVelocity();
            SetMomentum();
    
            CalculateMomentum();
            CalculateMovementVelocity();
    
            ApplyMovementVelocity();
    
        }
    
        void SetDirection()
        {
            if (currentDirection != movement.direction)
            {
                clearMomentum    = movement.direction != Vector2.zero;
                currentDirection = movement.direction;
            }
        }
    
        void SetVelocity()
        {
            movement.velocity       = currentDirection * attribute.speed ;
            movement.baseVelocity   = movement.velocity;
        }
    
        void SetMomentum()
        {
            if (clearMomentum)
            {
                movement.momentum = Vector2.zero;
                clearMomentum     = false;
            }
        }
    
        void CalculateMovementVelocity()
        {
            movement.velocity       += movement.momentum;
            movement.finalVelocity   = movement.velocity;
        }
    
    
        void ApplyMovementVelocity()
        {
            body.linearVelocity = movement.finalVelocity;
        }
    
    
    
        void CalculateMomentum()
        {
            if (movement.locomotion)
            {
                ApplyMomentum();
            }
            else
            {
                ApplyFriction();
            }
        }
    
        void ApplyMomentum()
        {
            movement.momentum += attribute.groundControlRate * Time.fixedDeltaTime * movement.baseVelocity;
            movement.momentum = Vector2.ClampMagnitude(movement.momentum, attribute.speed);
        }
    
        void ApplyFriction()
        {
            movement.momentum = Vector2.MoveTowards(movement.momentum, Vector2.zero, attribute.groundFriction * Time.fixedDeltaTime);
        }
    
    }
}
