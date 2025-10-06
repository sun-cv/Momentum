

using UnityEngine;

namespace Momentum
{

    public class Button
    {
        public ButtonInput     Input            { get; }
        public ButtonCondition Condition
        {
            get
            {
                if (pressedThisFrame)           return ButtonCondition.PressedThisFrame;
                if (pressedHeld)                return ButtonCondition.Held;
                if (pressed)                    return ButtonCondition.Pressed;
                if (releasedThisFrame)          return ButtonCondition.ReleasedThisFrame;
                if (releasedTime.IsRunning)     return ButtonCondition.ReleasedRecently;
                                                return ButtonCondition.None;
            }
        }
        
        public int        framePressed;
        public int        frameReleased;

        public StateFlag  pressed               = new();
        public StateFlag  pressedHeld           = new();
        public StateFlag  pressedThisFrame      = new();
        public StateFlag  releasedThisFrame     = new();
        public Stopwatch  pressedTime           = new();
        public Stopwatch  releasedTime          = new();

        public float      lastPressed;
        public float      lastReleased;


        public Button(ButtonInput input)        => Input = input;
        public ButtonPredicate predicate        => new(){input = Input, condition = Condition };
    }


}