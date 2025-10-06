using System;
using Momentum.HSM.Hero.Behavior;
using Momentum.HSM.Hero.Movement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Momentum
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class Hero : Entity
    {
        public InputRouterComponent     input;
        public AbilityComponent         ability;
        public CommandComponent         command;
        public AttributeComponent       attribute;
        public MovementEngineComponent  movement;
        public StateBehaviorComponent   movementState;
        public StateBehaviorComponent   behaviorState;


        public Context context;

        public void Awake()
        {
            CreateContext();

            input           .Initialize();
            ability         .Initialize(context);
            command         .Initialize(context, context.entity.Hero.Intent.input);
            attribute       .Initialize();
            movement        .Initialize(context, context.entity.Hero.Intent.movement);

            movementState   .Initialize(new RootMovement(context));
            behaviorState   .Initialize(new RootBehavior(context));
        }
        
        public void OnEnable() 
        {
            command.System  .AssignAbilitySystem(ability.System);
            command.System  .AssignAbilityMap(ability.Abilities);
        }

        public void Update()
        {
            input.Router    .Tick();

            command.System  .Tick();
            ability.System  .Tick();

            movementState   .Tick();
            behaviorState   .Tick();

            DebugButton();
        }

        public void FixedUpdate()
        {
            movement.Engine.TickFixed();
        }


        public void OnDisable() 
        {
            input.Router.OnDisable();
        }


        void CreateContext()
        {
            context = new()
            {
                component  = new ComponentContext()
                {
                    input       = input,
                    ability     = ability,
                    command     = command,
                    attribute   = attribute,
                    movement    = movement,
                },
                entity  = new HeroContext()
                {
                    self        = this.gameObject,
                    body        = new EntityBodyContext()
                    {
                        transform       = transform,
                        rigidBody       = GetComponent<Rigidbody2D>(),
                        colliderCapsule = GetComponent<CapsuleCollider2D>()
                    },
                    movement    = new EntityMovementContext()
                    {
                        intent  = input.Router.MovementIntent,
                    },
                    Intent      = new HeroIntent()
                    {
                        input       = input.Router.InputIntent,
                        movement    = input.Router.MovementIntent,
                    },
                },
            };
        }

        void DebugButton()
        {
            foreach (var button in input.Router.ActiveButtons)
            {
                Logwin.Log($"Button: {button.Input}", button.Condition, "Active buttons");
            }

        }

    }
}
