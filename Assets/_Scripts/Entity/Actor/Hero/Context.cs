using UnityEngine;


namespace Momentum
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

            public RequestFlag impulseRequest           = new();

            public CardinalDirection  cardinal;
            public PrincipalDirection principal;

            public MovementMode mode;
            public MovementIntent intent;
            public MovementCondition condition;

            public float speed;
            public float distance;
            public float progress;
            public float force;

            public Vector2 direction;
            public Vector2 defaultDirection             = Vector2.down;
            public Vector2 lastDirection;
            public Vector2 targetDirection;
            public Vector2 impulseDirection;
            public Vector2 lockedDirection;
            
            public Vector2 cardinalDirection;
            public Vector2 principalDirection;

            public Vector2 momentum;
            public Vector2 velocity;
            public Vector2 baseVelocity;
            public Vector2 finalVelocity;
            
            public Vector2 position;
            public Vector2 startPosition;
            public Vector2 desiredPosition;
            public Vector2 targetPosition;
            public Vector2 endPosition;

            public Stopwatch IdleTimer                  = new();

            public Movement()
            {
                idle        = new(() => direction == Vector2.zero);
                locomotion  = new(() => direction != Vector2.zero);
            }

            public void Set(MovementMode mode, MovementIntent intent) { this.mode = mode; this.intent = intent; } 

        }
   
        public class Action
        {

            public Dash dash                            = new();
            public BasicAttack basicAttack              = new(); 


            public class Dash
            {
                public RequestFlag request              = new();

                public float distance;

                public StatusFlag dashCooldown          = new();
            
                public Timer dashTimer;
                public Timer dashCooldownTimer;        
            }

            public class BasicAttack
            {
                
                public RequestFlag request              = new();
            
                public float attackCount;
           
                public StatusFlag attackCooldown        = new();
                public StatusFlag attackComboCooldown   = new();
            
                public Timer attackIntervalTimer;
                public Timer attackCooldownTimer;
                public Timer attackComboCooldownTimer;
                
            }

            public ShieldBlock shieldBlock              = new();

            public class ShieldBlock
            {

            }
        }
    }
}