using UnityEngine;



namespace Momentum
{

    public class InputRouter
    {
        HeroContext.Input       input;
        HeroContext.Movement    movement;

        IInputDriverMono        inputDriver;
        ICommandDispatcher      commandDispatcher;

        EventBinding<MouseClickLeft>        mouseClickLeft;
        EventBinding<MouseClickRight>       mouseClickRight;
        EventBinding<MouseClickLeftCancel>  mouseClickLeftCancel;
        EventBinding<MouseClickRightCancel> mouseClickRightCancel;
        EventBinding<DashInput> dashInput;
    

        public void Initialize(HeroContext context)
        {
            input               = context.input;
            movement            = context.movement;
            inputDriver         = Registry.Get<IInputDriverMono>();
            commandDispatcher   = Registry.Get<ICommandDispatcher>();
        }

        public void Tick()
        {
            input.mousePosition             = inputDriver.GetMousePosition();
            movement.direction              = inputDriver.GetMovement();

            if (movement.direction != Vector2.zero)
            {
                movement.cardinal           = DirectionUtility.GetCardinalDirection(movement.direction);
                movement.principal          = DirectionUtility.GetPrincipalDirection(movement.direction);
                movement.cardinalDirection  = DirectionUtility.GetDirectionVector(movement.cardinal);
                movement.principalDirection = DirectionUtility.GetDirectionVector(movement.principal);
                movement.lastDirection      = movement.direction;
            }
        }

        public void OnEnable()
        {
            mouseClickLeft                  = EventBus<MouseClickLeft>.Subscribe(HandleMouseClickLeft);        
            mouseClickRight                 = EventBus<MouseClickRight>.Subscribe(HandleMouseClickRight);       
            mouseClickLeftCancel            = EventBus<MouseClickLeftCancel>.Subscribe(HandleMouseClickLeftCancel); 
            mouseClickRightCancel           = EventBus<MouseClickRightCancel>.Subscribe(HandleMouseClickRightCancel); 
            dashInput                       = EventBus<DashInput>.Subscribe(HandleDashInput);             
        }

        public void OnDisable()
        {
            EventBus<MouseClickLeft>        .Unsubscribe(mouseClickLeft);
            EventBus<MouseClickRight>       .Unsubscribe(mouseClickRight);
            EventBus<MouseClickLeftCancel>  .Unsubscribe(mouseClickLeftCancel);
            EventBus<MouseClickRightCancel> .Unsubscribe(mouseClickRightCancel);
            EventBus<DashInput>             .Unsubscribe(dashInput);
        }

        void HandleMouseClickLeft(MouseClickLeft click)
        {
            input.leftClick.Set();
            commandDispatcher.Enqueue(Command.Create<BasicAttackCommand>());
        }

        void HandleMouseClickRight(MouseClickRight click)
        {
            input.rightClick.Set();
        }      
  
        void HandleMouseClickLeftCancel(MouseClickLeftCancel click)
        {
            input.leftClick.Clear();
        }  
        void HandleMouseClickRightCancel(MouseClickRightCancel click)
        {
            input.rightClick.Clear();
        } 
        void HandleDashInput(DashInput dashInput)
        {
            commandDispatcher.Enqueue(Command.Create<DashCommand>());
        }       
        
    }
}