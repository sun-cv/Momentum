// using Game.Common;
// using Game.Diagnostic;
// using UnityEngine;
// using UnityEngine.InputSystem;
//
//
// namespace Game.Interface
// {
//
//     public class InputDriver : RegisteredService, IRealBase
//     {
//         private InputActions input;
//
//         public InputDriver()
//         {
//             input = new();
//             input.Enable();
//
//             input.Player.Interact.performed += OnInteractPress;
//             input.Player.Interact.canceled  += OnInteractRelease;
//
//             input.Player.Action.performed   += OnActionPress;
//             input.Player.Action.canceled    += OnActionRelease;
//
//             input.Player.Attack1.performed  += OnAttack1Press;
//             input.Player.Attack1.canceled   += OnAttack1Release;
//
//             input.Player.Attack2.performed  += OnAttack2Press;
//             input.Player.Attack2.canceled   += OnAttack2Release;
//
//             input.Player.Modifier.performed += OnModifierPress;
//             input.Player.Modifier.canceled  += OnModifierRelease;
//
//             input.Player.Dodge.performed    += OnDodgePress;
//             input.Player.Dodge.canceled     += OnDodgeRelease;
//         }
//
//         public void Tick()
//         {
//             PollContinousInputs();
//         }
//
//         void PollContinousInputs()
//         {
//             PollMousePosition();
//             PollMovementVector();
//         }
//
//         void PollMousePosition()    => Common.Event.Send<MousePosition> (new MousePosition(input.Player.Mouse.ReadValue<Vector2>()));
//         void PollMovementVector()   => Common.Event.Send<MovementVector>(new MovementVector(input.Player.Move.ReadValue<Vector2>()));
//
//         void OnInteractPress   (InputAction.CallbackContext context) => Common.Event.Send<InteractPress>  (new());
//         void OnInteractRelease (InputAction.CallbackContext context) => Common.Event.Send<InteractRelease>(new());
//
//         void OnActionPress     (InputAction.CallbackContext context) => Common.Event.Send<ActionPress>    (new());
//         void OnActionRelease   (InputAction.CallbackContext context) => Common.Event.Send<ActionRelease>  (new());
//
//         void OnAttack1Press    (InputAction.CallbackContext context) => Common.Event.Send<Attack1Press>   (new());
//         void OnAttack1Release  (InputAction.CallbackContext context) => Common.Event.Send<Attack1Release> (new());
//
//         void OnAttack2Press    (InputAction.CallbackContext context) => Common.Event.Send<Attack2Press>   (new());
//         void OnAttack2Release  (InputAction.CallbackContext context) => Common.Event.Send<Attack2Release> (new());
//
//         void OnModifierPress   (InputAction.CallbackContext context) => Common.Event.Send<ModifierPress>  (new());
//         void OnModifierRelease (InputAction.CallbackContext context) => Common.Event.Send<ModifierRelease>(new());
//
//         void OnDodgePress      (InputAction.CallbackContext context) => Common.Event.Send<DashPress>      (new());
//         void OnDodgeRelease    (InputAction.CallbackContext context) => Common.Event.Send<DashRelease>    (new());
//
//         public override void OnDispose()
//         {
//             input?.Player.Disable();
//             input?.Disable();
//             input?.Dispose();
//             input = null;
//         }
//
//         static InputDriver() => Log<InputDriver>.Level(Diagnostic.Log.Level.Debug); 
//     }
//
//     public class Button
//     {
//         bool pressed                            = false;
//         bool released                           = false;
//         bool pressedThisFrame                   = false;
//         bool releasedThisFrame                  = false;
//
//         // -----------------------------------
//
//         readonly FrameWatch pressedframeCount   = new();
//         readonly FrameWatch releasedframeCount  = new();
//
//         // ===============================================================================
//
//         public Button(Capability input)
//         {    
//             Input = input;
//         }
//
//         // ===============================================================================
//
//         public void Press()
//         {
//             ResetState();
//
//             pressedThisFrame  = true;
//             pressed           = true;
//             pressedframeCount.Start();
//         }
//
//         public void Release()
//         {
//             pressedframeCount.Stop();
//
//             pressed           = false;
//             releasedThisFrame = true;
//             released          = true;
//
//             releasedframeCount.Start();
//         }
//
//         public void Update()
//         {
//             if (pressedThisFrame && pressedframeCount.CurrentFrame != 0)
//             {
//                 pressedThisFrame  = false;
//             }
//
//             if (releasedThisFrame && releasedframeCount.CurrentFrame != 0)
//             {
//                 releasedThisFrame = false;
//             }
//
//             if (releasedframeCount.CurrentFrame >= Config.Input.RELEASE_THRESHOLD)
//             {
//                 releasedframeCount.Stop();
//                 released = false;
//             }
//         }
//
//         void ResetState()
//         {
//
//             pressed             = false;
//             released            = false;
//             pressedThisFrame    = false;
//             releasedThisFrame   = false;
//
//
//             pressedframeCount.Stop();
//             pressedframeCount.Reset();
//
//             releasedframeCount.Stop();
//             releasedframeCount.Reset();   
//         }
//
//         // ===============================================================================
//
//         public Capability    Input            { get; }
//         public InputCondition Condition
//         {
//             get
//             {
//                 if (pressedThisFrame)           return InputCondition.PressedThisFrame;
//                 if (pressed)                    return InputCondition.Pressed;
//                 if (releasedThisFrame)          return InputCondition.ReleasedThisFrame;
//                 if (released)                   return InputCondition.ReleasedRecently;
//                 return InputCondition.None;
//             }
//         }
//         public bool Pressed                     => pressed; 
//         public bool Released                    => released; 
//         public bool PressedThisFrame            => pressedThisFrame; 
//         public bool ReleasedThisFrame           => releasedThisFrame; 
//
//         public FrameWatch PressedframeCount     => pressedframeCount;  
//         public FrameWatch ReleasedframeCount    => releasedframeCount; 
//     }
//
//
//     public struct PendingInputEvent
//     {
//         public Capability Action      { get; set; }
//         public bool IsPress             { get; set; }
//     }
//
//     public interface IInputEvent : IEvent { Capability Action { get; } InputCondition Condition { get; }}
//
//     public struct InteractPress         : IInputEvent { public readonly Capability Action => Capability.Interact; public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
//     public struct InteractRelease       : IInputEvent { public readonly Capability Action => Capability.Interact; public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }
//
//     public struct ActionPress           : IInputEvent { public readonly Capability Action => Capability.Action;   public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
//     public struct ActionRelease         : IInputEvent { public readonly Capability Action => Capability.Action;   public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }
//
//     public struct Attack1Press          : IInputEvent { public readonly Capability Action => Capability.Attack1;  public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
//     public struct Attack1Release        : IInputEvent { public readonly Capability Action => Capability.Attack1;  public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }
//
//     public struct Attack2Press          : IInputEvent { public readonly Capability Action => Capability.Attack2;  public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
//     public struct Attack2Release        : IInputEvent { public readonly Capability Action => Capability.Attack2;  public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }
//
//     public struct ModifierPress         : IInputEvent { public readonly Capability Action => Capability.Modifier; public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
//     public struct ModifierRelease       : IInputEvent { public readonly Capability Action => Capability.Modifier; public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }
//
//     public struct DashPress             : IInputEvent { public readonly Capability Action => Capability.Dash;     public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
//     public struct DashRelease           : IInputEvent { public readonly Capability Action => Capability.Dash;     public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }
//
//     public struct MousePosition         : IInputEvent { public readonly Capability Action => Capability.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MousePosition(Vector2 value) => vector = value;   }
//     public struct MovementVector        : IInputEvent { public readonly Capability Action => Capability.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MovementVector(Vector2 value) => vector = value;  }
//
//
// }
//
//
