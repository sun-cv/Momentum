using UnityEngine;
using Momentum.Markers;
using Momentum.Definition;
using Momentum.Timers;



namespace Momentum.Actor.Hero
{

    public class HeroContext
    {
        public Transform            transform;
        public Rigidbody2D          body;
        public CapsuleCollider2D    collider;
    
        public HeroAttributes       attributes;

        public Input            input                   = new();
        public State            state                   = new();
        public Condition        condition               = new();
        public Movement         movement                = new();
        public Action           action                  = new();
    
        public class Input
        {
            public StatusFlag leftClick                 = new();
            public StatusFlag rightClick                = new();


            public Vector2 mousePosition;
        }

        public class State
        {
            public StatusFlag TransitionLocked          = new();
            public StatusFlag idle                      = new();
            public StatusFlag sprint                    = new();
            public StatusFlag dash                      = new();
            public StatusFlag basicAttack               = new();
            public StatusFlag shieldBlock               = new();
        }   


        public class Condition  
        {               
            public StatusFlag disabled                  = new();

            public StatusFlag interrupted               = new();
            public StatusFlag stunned                   = new();
            public StatusFlag knockedBack               = new();
            public StatusFlag slowed                    = new();
        }

        public class Movement
        {
            public AutoBool idle;
            public AutoBool locomotion;

            public CardinalDirection    cardinalDirection;
            public PrincipalDirection   principalDirection;

            public float speed;

            public Vector2 direction;
            public Vector2 directionNormalized;
            public Vector2 cardinal;
            public Vector2 principal;

            public Vector2 momentum;
            public Vector2 velocity;
            public Vector2 baseVelocity;
            public Vector2 finalVelocity;
            
            public Stopwatch IdleTimer                  = new();

            public Movement()
            {
                idle        = new(() => direction == Vector2.zero);
                locomotion  = new(() => direction != Vector2.zero);
            }
        }
   
        public class Action
        {

            public Dash dash                            = new();

            public class Dash
            {
                public RequestFlag request              = new();

                public float velocity;
                public float distance;
                public float defaultVelocity= 1;

                public Vector2 direction;
                public Vector2 startPosition;
                public Vector2 targetPosition;

                public StatusFlag dashCooldown          = new();
            
                public Timer dashTimer;
                public Timer dashCooldownTimer;

                public Dash()
                {
                    velocity = defaultVelocity;
                }                
            }

            public BasicAttack basicAttack              = new(); 

            public class BasicAttack
            {
                
                public RequestFlag request              = new();
            
                public float attackCount;

                public float velocity;
                public float distance;
                public float defaultVelocity= 1;

                public Vector2 direction;
                public Vector2 startPosition;
                public Vector2 targetPosition;
            
            
                public StatusFlag attackCooldown        = new();
                public StatusFlag attackComboCooldown   = new();
            
                public Timer attackIntervalTimer;
                public Timer attackCooldownTimer;
                public Timer attackComboCooldownTimer;
                
                public BasicAttack()
                {
                    velocity = defaultVelocity;
                }
            }

            public ShieldBlock shieldBlock              = new();

            public class ShieldBlock
            {

            }
        }
    }
}