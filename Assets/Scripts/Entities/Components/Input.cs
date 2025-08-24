using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;


namespace Momentum
{




    public interface IInput
    {
        public Vector2      GetMousePosition();
        public Vector2      GetMovementVector();
        public List<Button> GetActiveButtons();

        public void BindMovementIntent(MovementIntent intent);
    }

    public class InputComponent : IInput
    {
        private IInput system = new Input();
    }


    public class Input : IInput
    {
        MovementIntent movementIntent;

        EventBinding<InteractPress>         interactPress;
        EventBinding<InteractRelease>       interactRelease;
        EventBinding<AttackPress>           attackPress;
        EventBinding<AttackRelease>         attackRelease;
        EventBinding<BlockPress>            blockPress;
        EventBinding<BlockRelease>          blockRelease;
        EventBinding<DashPress>             dashPress;
        EventBinding<DashRelease>           dashRelease;
        EventBinding<AimPress>              aimPress;
        EventBinding<AimRelease>            aimRelease;
    
        private Dictionary<ButtonInput, Button> buttonMap;
        private List<Button> activeButtons = new();

        float MinimumHoldThreshold = 0.225f;

        public void OnEnable()
        {
            interactPress                   = BindPress     <InteractPress>(); 
            interactRelease                 = BindRelease   <InteractRelease>();
            attackPress                     = BindPress     <AttackPress>(); 
            attackRelease                   = BindRelease   <AttackRelease>();
            blockPress                      = BindPress     <BlockPress>();     
            blockRelease                    = BindRelease   <BlockRelease>();
            dashPress                       = BindPress     <DashPress>();           
            dashRelease                     = BindRelease   <DashRelease>();  
            aimPress                        = BindPress     <AimPress>();           
            aimRelease                      = BindRelease   <AimRelease>();              
        }

        public void OnDisable()
        {
            EventBus<AttackPress>           .Unsubscribe(attackPress);
            EventBus<AttackRelease>         .Unsubscribe(attackRelease);
            EventBus<BlockPress>            .Unsubscribe(blockPress);
            EventBus<BlockRelease>          .Unsubscribe(blockRelease);
            EventBus<DashPress>             .Unsubscribe(dashPress);
            EventBus<DashRelease>           .Unsubscribe(dashRelease);
        }


        public void Update()
        {
            UpdateButtons();
            UpdateMovementIntent();
        }
        
        private EventBinding<T> BindPress<T>() where T : IInputEvent
        {
            return EventBus<T>.Subscribe(evt => { HandleButtonPress(GetOrCreateButton(evt.Button));});
        }

        private EventBinding<T> BindRelease<T>() where T : IInputEvent
        {
            return EventBus<T>.Subscribe(evt => { HandleButtonRelease(GetOrCreateButton(evt.Button)); });
        }

        private Button GetOrCreateButton(ButtonInput input)
        {
            if (!buttonMap.TryGetValue(input, out Button button))
            {
                button = new Button();
            }
            return button;
        }

        void HandleButtonPress(Button button)
        {
            button.pressed.Set();
            button.pressedTime.Start();
            button.pressedThisFrame.Set();
            button.lastPressed  = Time.time;

            if (!activeButtons.Contains(button))
            {
                activeButtons.Add(button);
            }
        }

        void HandleButtonRelease(Button button)
        {
            button.pressed.Clear();
            button.pressedHeld.Clear();
            button.pressedTime.Stop().Reset();

            button.releasedThisFrame.Set();
            button.lastReleased = Time.time;
        }


        void UpdateButtons()
        {
            for (int i = activeButtons.Count - 1; i >= 0; i--)
            {
                var button = activeButtons[i];
                UpdateButtonPressContext(button);

                if (!button.pressed && !button.pressedHeld && button.releasedTime.CurrentTime <= 0f)
                {
                    activeButtons.RemoveAt(i);
                }
            }
        }

        void UpdateButtonPressContext(Button button)
        {
            if (button.pressedThisFrame && button.pressedTime.CurrentTime > 0f)
            {
                button.pressedThisFrame.Clear();
                return;
            }
            if (button.pressed && !button.pressedHeld && button.pressedTime.CurrentTime >= MinimumHoldThreshold )
            {
                button.pressedHeld.Set();
                return;
            }
            if (button.releasedThisFrame && button.releasedTime.CurrentTime <= 0f )
            {
                button.releasedTime.Start();
                return;
            }
            else if (button.releasedThisFrame && button.releasedTime.CurrentTime > 0f)
            {
                button.releasedThisFrame.Clear();
            }
            if (button.releasedTime.CurrentTime >= GameDefault.ButtonReleaseBuffer)
            {
                button.releasedTime.Stop().Reset();
            }
        }   

        void UpdateMovementIntent()
        {
            if (movementIntent == null) return;

        }

        public Vector2      GetMousePosition()  { return Vector2.left; }
        public Vector2      GetMovementVector() { return Vector2.left; }
        public List<Button> GetActiveButtons()  { return activeButtons; }
        public void BindMovementIntent(MovementIntent intent) => movementIntent = intent;
    }
}