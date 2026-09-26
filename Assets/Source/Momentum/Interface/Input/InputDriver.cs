using System.Collections.Generic;
using Game.Common;
using Game.Common.Events;
using Game.Diagnostic;
using UnityEngine;
using UnityEngine.InputSystem;
using Event = Game.Common.Event;



namespace Game.Interface
{
    public class InputDriver : RegisteredService, IRealBase
    {
        private Camera view;
        private InputActions input;
        private readonly Dictionary<InputAction, Capability> map = new();

        public InputDriver()
        {
            input = new();
            input.Enable();

            map[input.Player.Interact]  = Capability.Interact;
            map[input.Player.Action]    = Capability.Action;
            map[input.Player.Primary]   = Capability.Primary;
            map[input.Player.Secondary] = Capability.Secondary;
            map[input.Player.Modifier]  = Capability.Modifier;
            map[input.Player.Dodge]     = Capability.Dodge;

            input.Player.Get().actionTriggered += OnAction;

            Event.Register<CameraCreated>(AssignCamera);            
        }

        void IRealBase.Tick()
        {
            PollIntentVector();
            PollMousePosition();
        }

        void PollMousePosition()
        {
            var mouse   = input.Player.Aim.ReadValue<Vector2>();
            var cursor  = (Vector2)view.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 0f));

            Event.Send(new MousePosition()  { World = cursor, Screen = mouse });
        }

        void PollIntentVector()
        {
            Event.Send(new MovementIntent() { Vector = input.Player.Move.ReadValue<Vector2>()});
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

            Event.Send(new InputEvent() { Capability = capability, Pressed = condition, Released = !condition });
        }

        private void AssignCamera(CameraCreated message)
        {
            view = message.View;
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


