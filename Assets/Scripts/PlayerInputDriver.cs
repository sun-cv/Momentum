using System;
using UnityEngine;
using UnityEngine.InputSystem;



public class InputDriver : RegisteredService, IServiceTick, IInitialize, IDisposable
{
    private InputActions input;

    // ===============================================================================

    public void Initialize()
    {
        input = new();
        input.Enable();

        input.Player.Interact.performed += OnInteractPress;
        input.Player.Interact.canceled  += OnInteractRelease;

        input.Player.Action.performed   += OnActionPress;
        input.Player.Action.canceled    += OnActionRelease;

        input.Player.Attack1.performed  += OnAttack1Press;
        input.Player.Attack1.canceled   += OnAttack1Release;

        input.Player.Attack2.performed  += OnAttack2Press;
        input.Player.Attack2.canceled   += OnAttack2Release;

        input.Player.Modifier.performed += OnModifierPress;
        input.Player.Modifier.canceled  += OnModifierRelease;

        input.Player.Dodge.performed    += OnDodgePress;
        input.Player.Dodge.canceled     += OnDodgeRelease;

    }

    // ===============================================================================
    
    public void Tick()
    {
        PollContinousInputs();
    }

    void PollContinousInputs()
    {
        PollMousePosition();
        PollMovementVector();
    }
    
    void PollMousePosition()    => EventBus<MousePosition> .Raise(new MousePosition(input.Player.Mouse.ReadValue<Vector2>()));
    void PollMovementVector()   => EventBus<MovementVector>.Raise(new MovementVector(input.Player.Move.ReadValue<Vector2>()));

    // ===============================================================================
    //  Events
    // ===============================================================================

    void OnInteractPress   (InputAction.CallbackContext context) => EventBus<InteractPress>     .Raise(new());
    void OnInteractRelease (InputAction.CallbackContext context) => EventBus<InteractRelease>   .Raise(new());

    void OnActionPress     (InputAction.CallbackContext context) => EventBus<ActionPress>       .Raise(new());
    void OnActionRelease   (InputAction.CallbackContext context) => EventBus<ActionRelease>     .Raise(new());

    void OnAttack1Press    (InputAction.CallbackContext context) => EventBus<Attack1Press>      .Raise(new());
    void OnAttack1Release  (InputAction.CallbackContext context) => EventBus<Attack1Release>    .Raise(new());

    void OnAttack2Press    (InputAction.CallbackContext context) => EventBus<Attack2Press>      .Raise(new());
    void OnAttack2Release  (InputAction.CallbackContext context) => EventBus<Attack2Release>    .Raise(new());

    void OnModifierPress   (InputAction.CallbackContext context) => EventBus<ModifierPress>     .Raise(new());
    void OnModifierRelease (InputAction.CallbackContext context) => EventBus<ModifierRelease>   .Raise(new());

    void OnDodgePress      (InputAction.CallbackContext context) => EventBus<DashPress>         .Raise(new());
    void OnDodgeRelease    (InputAction.CallbackContext context) => EventBus<DashRelease>       .Raise(new());

    // ===============================================================================

    public override void Dispose()
    {
        input?.Player.Disable();
        input?.Disable();
        input?.Dispose();
        input = null;
    }

    public UpdatePriority Priority => ServiceUpdatePriority.InputDriver;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class InputButton
{
    bool pressed                            = false;
    bool released                           = false;
    bool pressedThisFrame                   = false;
    bool releasedThisFrame                  = false;

    // -----------------------------------

    readonly FrameWatch pressedframeCount   = new();
    readonly FrameWatch releasedframeCount  = new();

    // ===============================================================================

    public InputButton(Capability input)
    {    
        Input = input;
    }

    // ===============================================================================

    public void Press()
    {
        ResetState();

        pressedThisFrame  = true;
        pressed           = true;
        pressedframeCount.Start();
    }

    public void Release()
    {
        pressedframeCount.Stop();

        pressed           = false;
        releasedThisFrame = true;
        released          = true;

        releasedframeCount.Start();
    }

    public void Update()
    {
        if (pressedThisFrame && pressedframeCount.CurrentFrame != 0)
        {
            pressedThisFrame    = false;
        }

        if (releasedThisFrame && releasedframeCount.CurrentFrame != 0)
        {
            releasedThisFrame   = false;
        }

        if (releasedframeCount.CurrentFrame >= Config.Input.RELEASE_THRESHOLD)
        {
            releasedframeCount.Stop();
            released = false;
        }
    }

    void ResetState()
    {

        pressed             = false;
        released            = false;
        pressedThisFrame    = false;
        releasedThisFrame   = false;


        pressedframeCount.Stop();
        pressedframeCount.Reset();

        releasedframeCount.Stop();
        releasedframeCount.Reset();   
    }

    // ===============================================================================

    public Capability    Input            { get; }
    public InputCondition Condition
    {
        get
        {
            if (pressedThisFrame)           return InputCondition.PressedThisFrame;
            if (pressed)                    return InputCondition.Pressed;
            if (releasedThisFrame)          return InputCondition.ReleasedThisFrame;
            if (released)                   return InputCondition.ReleasedRecently;
                                            return InputCondition.None;
        }
    }
    public bool Pressed                     => pressed; 
    public bool Released                    => released; 
    public bool PressedThisFrame            => PressedThisFrame; 
    public bool ReleasedThisFrame           => ReleasedThisFrame; 

    public FrameWatch PressedframeCount     => pressedframeCount;  
    public FrameWatch ReleasedframeCount    => releasedframeCount; 
}


public struct PendingInputEvent
{
    public Capability Action      { get; set; }
    public bool IsPress             { get; set; }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IInputEvent : IEvent { Capability Action { get; } InputCondition Condition { get; }}

public struct InteractPress         : IInputEvent { public readonly Capability Action => Capability.Interact; public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct InteractRelease       : IInputEvent { public readonly Capability Action => Capability.Interact; public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct ActionPress           : IInputEvent { public readonly Capability Action => Capability.Action;   public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct ActionRelease         : IInputEvent { public readonly Capability Action => Capability.Action;   public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct Attack1Press          : IInputEvent { public readonly Capability Action => Capability.Attack1;  public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct Attack1Release        : IInputEvent { public readonly Capability Action => Capability.Attack1;  public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct Attack2Press          : IInputEvent { public readonly Capability Action => Capability.Attack2;  public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct Attack2Release        : IInputEvent { public readonly Capability Action => Capability.Attack2;  public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct ModifierPress         : IInputEvent { public readonly Capability Action => Capability.Modifier; public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct ModifierRelease       : IInputEvent { public readonly Capability Action => Capability.Modifier; public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct DashPress             : IInputEvent { public readonly Capability Action => Capability.Dash;     public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct DashRelease           : IInputEvent { public readonly Capability Action => Capability.Dash;     public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct MousePosition         : IInputEvent { public readonly Capability Action => Capability.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MousePosition(Vector2 value) => vector = value;   }
public struct MovementVector        : IInputEvent { public readonly Capability Action => Capability.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MovementVector(Vector2 value) => vector = value;  }

