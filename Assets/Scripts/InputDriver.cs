using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;






public class InputDriver : RegisteredService, IServiceTick, IDisposable
{
    private InputActions input;

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


    public override void Initialize()
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

    public void Dispose()
    {
        input?.Player.Disable();
        input?.Disable();
        input?.Dispose();
        input = null;
    }

    public UpdatePriority Priority => ServiceUpdatePriority.InputDriver;
}

public struct PendingInputEvent
{
    public InputIntent Intent   { get; set; }
    public bool IsPress         { get; set; }
}


public class InputRouter : RegisteredService, IServiceTick, IDisposable
{

    Dictionary<InputIntent, InputButton> buttonMap  = new();
    HashSet<InputButton> activeButtons              = new();
    Queue<PendingInputEvent> pendingInputs          = new();

    Vector2 mousePositionVector;
    Vector2 movementDirectionVector;

    EventBinding<InteractPress>         interactPress;
    EventBinding<InteractRelease>       interactRelease;
    EventBinding<ActionPress>           actionPress;
    EventBinding<ActionRelease>         actionRelease;
    EventBinding<Attack1Press>          attack1Press;
    EventBinding<Attack1Release>        attack1Release;
    EventBinding<Attack2Press>          attack2Press;
    EventBinding<Attack2Release>        attack2Release;
    EventBinding<ModifierPress>         modifierPress;
    EventBinding<ModifierRelease>       modifierRelease;
    EventBinding<DashPress>             dashPress;
    EventBinding<DashRelease>           dashRelease;
    EventBinding<MovementVector>        movementVector;
    EventBinding<MousePosition>         mousePosition;

    public override void Initialize()
    {
        mousePosition   = EventBus<MousePosition> .Subscribe(UpdateMousePosition);
        movementVector  = EventBus<MovementVector>.Subscribe(UpdateMovementIntent);    

        interactPress   = BindPress     <InteractPress>     ();
        interactRelease = BindRelease   <InteractRelease>   ();
        actionPress     = BindPress     <ActionPress>       ();
        actionRelease   = BindRelease   <ActionRelease>     ();
        attack1Press    = BindPress     <Attack1Press>      ();
        attack1Release  = BindRelease   <Attack1Release>    ();
        attack2Press    = BindPress     <Attack2Press>      ();
        attack2Release  = BindRelease   <Attack2Release>    ();
        modifierPress   = BindPress     <ModifierPress>     ();
        modifierRelease = BindRelease   <ModifierRelease>   ();
        dashPress      = BindPress      <DashPress>        ();
        dashRelease    = BindRelease    <DashRelease>      ();

        foreach (InputIntent intent in EnumUtils.GetEnumValues<InputIntent>())
        {
            if (intent == InputIntent.None)
                continue;

            buttonMap[intent] = new InputButton(intent);
        }
    }

    public void Dispose()
    {
        EventBus<InteractPress>     .Unsubscribe(interactPress);  
        EventBus<InteractRelease>   .Unsubscribe(interactRelease);  
        EventBus<ActionPress>       .Unsubscribe(actionPress);  
        EventBus<ActionRelease>     .Unsubscribe(actionRelease);  
        EventBus<Attack1Press>      .Unsubscribe(attack1Press);  
        EventBus<Attack1Release>    .Unsubscribe(attack1Release);  
        EventBus<Attack2Press>      .Unsubscribe(attack2Press);  
        EventBus<Attack2Release>    .Unsubscribe(attack2Release);  
        EventBus<ModifierPress>     .Unsubscribe(modifierPress);  
        EventBus<ModifierRelease>   .Unsubscribe(modifierRelease);  
        EventBus<DashPress>         .Unsubscribe(dashPress);  
        EventBus<DashRelease>       .Unsubscribe(dashRelease);  
    }

    public void Tick()
    {
        ResolvePendingInput();
        UpdateButtons();
    }


    void ResolvePendingInput()
    {
        while(pendingInputs.TryDequeue(out var evt))
        {
            if (evt.IsPress)
                GetOrCreateButton(evt.Intent).Press();
            else
                GetOrCreateButton(evt.Intent).Release();
        }

    }

    InputButton GetOrCreateButton(InputIntent input)
    {
        if (!buttonMap.TryGetValue(input, out InputButton button))
        {
            button = new InputButton(input);
            buttonMap[input] = button;
        }

        return button;
    }

    void UpdateButtons()
    {
        foreach (var button in buttonMap.Values)
        {
            button.Update();

            if (button.Condition != InputCondition.None)
                activeButtons.Add(button);
            else
                activeButtons.Remove(button);
        }
    }

    void UpdateMousePosition(MousePosition evt)     => mousePositionVector      = evt.vector;
    void UpdateMovementIntent(MovementVector evt)   => movementDirectionVector  = evt.vector;

    EventBinding<T> BindPress<T>()   where T : IInputEvent => EventBus<T>.Subscribe(evt => { pendingInputs.Enqueue(new() { Intent = evt.Intent, IsPress = true  }); });
    EventBinding<T> BindRelease<T>() where T : IInputEvent => EventBus<T>.Subscribe(evt => { pendingInputs.Enqueue(new() { Intent = evt.Intent, IsPress = false }); });

    public Vector2 MousePosition     => mousePositionVector;
    public Vector2 MovementDirection => movementDirectionVector;

    public Dictionary<InputIntent, InputButton> ButtonMap   => buttonMap;
    public HashSet<InputButton> ActiveButtons               => activeButtons;
    
    public UpdatePriority Priority => ServiceUpdatePriority.InputRouter;
}


public class InputButton
{
    public InputIntent    Input            { get; }
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

    public bool pressed                     = false;
    public bool released                    = false;
    public bool pressedThisFrame            = false;
    public bool releasedThisFrame           = false;

    public FrameWatch pressedframeCount   = new();
    public FrameWatch releasedframeCount  = new();

    public InputButton(InputIntent input)   => Input = input;

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
}



public interface IInputEvent : IEvent { InputIntent Intent { get; } InputCondition Condition { get; }}

public struct InteractPress         : IInputEvent { public readonly InputIntent Intent => InputIntent.Interact; public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct InteractRelease       : IInputEvent { public readonly InputIntent Intent => InputIntent.Interact; public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct ActionPress           : IInputEvent { public readonly InputIntent Intent => InputIntent.Action;   public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct ActionRelease         : IInputEvent { public readonly InputIntent Intent => InputIntent.Action;   public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct Attack1Press          : IInputEvent { public readonly InputIntent Intent => InputIntent.Attack1;  public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct Attack1Release        : IInputEvent { public readonly InputIntent Intent => InputIntent.Attack1;  public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct Attack2Press          : IInputEvent { public readonly InputIntent Intent => InputIntent.Attack2;  public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct Attack2Release        : IInputEvent { public readonly InputIntent Intent => InputIntent.Attack2;  public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct ModifierPress         : IInputEvent { public readonly InputIntent Intent => InputIntent.Modifier; public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct ModifierRelease       : IInputEvent { public readonly InputIntent Intent => InputIntent.Modifier; public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct DashPress             : IInputEvent { public readonly InputIntent Intent => InputIntent.Dash;     public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct DashRelease           : IInputEvent { public readonly InputIntent Intent => InputIntent.Dash;     public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct MousePosition         : IInputEvent { public readonly InputIntent Intent => InputIntent.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MousePosition(Vector2 value) => vector = value;   }
public struct MovementVector        : IInputEvent { public readonly InputIntent Intent => InputIntent.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MovementVector(Vector2 value) => vector = value;  }

