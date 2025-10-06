using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;


namespace Momentum
{
    public interface IInputRouter
    {
        public void Initialize();
        public void Tick();

        public void OnDisable();

        public Vector2          GetMousePosition();
        public Vector2          GetMovementVector();
        public HashSet<Button>  GetActiveButtons();

        public MovementIntent MovementIntent        { get; }
        public InputIntent InputIntent              { get; }
        public HashSet<Button> ActiveButtons        { get; }
    }


    public class InputRouter : IInputRouter
    {
        private MovementIntent movementIntent       = new();
        private InputIntent inputIntent             = new();

        private EventBinding<InteractPress>         interactPress;
        private EventBinding<InteractRelease>       interactRelease;
        private EventBinding<AttackPress>           attackPress;
        private EventBinding<AttackRelease>         attackRelease;
        private EventBinding<BlockPress>            blockPress;
        private EventBinding<BlockRelease>          blockRelease;
        private EventBinding<DashPress>             dashPress;
        private EventBinding<DashRelease>           dashRelease;
        private EventBinding<AimPress>              aimPress;
        private EventBinding<AimRelease>            aimRelease;
    
        private EventBinding<MousePosition>         mouseVector;
        private EventBinding<MovementVector>        movementVector;

        private Dictionary<ButtonInput, Button> buttonMap   = new();
        private HashSet<Button> activeButtons               = new();

        float MinimumHoldThreshold = 0.225f;

        public void Initialize()
        {
            inputIntent.activeButtons = activeButtons;

            mouseVector     = EventBus<MousePosition>.Subscribe(UpdateMousePosition);
            movementVector  = EventBus<MovementVector>.Subscribe(UpdateMovementIntent);    

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
            EventBus<MousePosition>         .Unsubscribe(mouseVector);
            EventBus<MovementVector>        .Unsubscribe(movementVector);

            EventBus<AttackPress>           .Unsubscribe(attackPress);
            EventBus<AttackRelease>         .Unsubscribe(attackRelease);
            EventBus<BlockPress>            .Unsubscribe(blockPress);
            EventBus<BlockRelease>          .Unsubscribe(blockRelease);
            EventBus<DashPress>             .Unsubscribe(dashPress);
            EventBus<DashRelease>           .Unsubscribe(dashRelease);
        }


        public void Tick()
        {
            UpdateButtons();
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
                button = new Button(input);
                buttonMap[input] = button;
            }
            return button;
        }

        void HandleButtonPress(Button button)
        {
            button.pressed.Set();
            button.pressedTime.Reset();
            button.pressedTime.Start();
            button.pressedThisFrame.Set();

            button.lastPressed  = Time.time;
            button.framePressed = Time.frameCount;
        }

        void HandleButtonRelease(Button button)
        {
            button.pressed.Clear();
            button.pressedHeld.Clear();
            button.pressedTime.Stop();

            button.releasedTime.Reset();
            button.releasedTime.Start();
            button.releasedThisFrame.Set();

            button.lastReleased = Time.time;
            button.frameReleased= Time.frameCount;
        }


        void UpdateButtons()
        {
            foreach (var (input, button) in buttonMap)
            {     
                if (button.Condition == ButtonCondition.None)
                {
                    activeButtons.Remove(button);
                    continue;
                }

                UpdateButtonPressContext(button);

                activeButtons.Add(button);      
            }
        }

        void UpdateButtonPressContext(Button button)
        {
            if (button.pressedThisFrame && button.framePressed != Time.frameCount)
            {
                button.pressedThisFrame.Clear();
                return;
            }
            if (button.pressed && !button.pressedHeld && button.pressedTime.CurrentTime >= MinimumHoldThreshold )
            {
                button.pressedHeld.Set();
                return;
            }
            if (button.releasedThisFrame && button.frameReleased != Time.frameCount)
            {
                button.releasedThisFrame.Clear();
                return;
            }
            if (button.releasedTime.CurrentTime >= GameDefault.ButtonReleaseBuffer)
            {
                button.releasedTime.Stop();
            }
        }   

        void UpdateMousePosition(MousePosition evt)     => inputIntent.mousePosition = evt.vector;
        void UpdateMovementIntent(MovementVector evt)   => movementIntent.direction  = evt.vector;


        public Vector2      GetMousePosition()      { return Vector2.left; }
        public Vector2      GetMovementVector()     { return Vector2.left; }
        public HashSet<Button> GetActiveButtons()   { return ActiveButtons; }

        public MovementIntent MovementIntent        => movementIntent;
        public InputIntent InputIntent              => inputIntent;
        public HashSet<Button> ActiveButtons        => activeButtons;

    }
}