using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;
using UnityEngine;
using UnityEngine.InputSystem;
using Event = Game.Common.Event;



namespace Game.Interface
{
    public class InputDriver : RegisteredService, IRealBase
    {
        private InputActions input;
        private readonly Dictionary<InputAction, Capability> map = new();

        public InputDriver()
        {
            input = new();
            input.Enable();

            map[input.Player.Interact] = Capability.Interact;
            map[input.Player.Action]   = Capability.Action;
            map[input.Player.Attack1]  = Capability.Attack1;
            map[input.Player.Attack2]  = Capability.Attack2;
            map[input.Player.Modifier] = Capability.Modifier;
            map[input.Player.Dodge]    = Capability.Dodge;

            input.Player.Get().actionTriggered += OnAction;
        }

        void IRealBase.Tick()
        {
            PollAimPosition();
            PollIntentVector();
        }

        void PollAimPosition()
        {
            Event.Send(new AimVector() { Vector = input.Player.Aim.ReadValue<Vector2>()});
        }

        void PollIntentVector()
        {
            Event.Send(new IntentVector() { Vector = input.Player.Move.ReadValue<Vector2>()});
        }

        void OnAction(InputAction.CallbackContext context)
        {
            if (!map.TryGetValue(context.action, out var capability))
                return;
            
            if (context.phase == InputActionPhase.Started)
                return;

            var condition = context.phase switch
            {
                InputActionPhase.Performed => true,
                InputActionPhase.Canceled  => false,
                _ => false,
            };

            Debug.Log(condition);

            Event.Send(new InputEvent() { Capability = capability, Pressed = condition, Released = !condition });
        }

        public override void OnDispose()
        {
            input.Player.Get().actionTriggered -= OnAction;
            input.Player.Disable();
            input.Disable();
            input = null;
        }

        static InputDriver() => Log<InputDriver>.Level(Diagnostic.Log.Level.Debug); 
    }

}


