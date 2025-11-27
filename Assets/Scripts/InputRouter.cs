using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[Service]
public class PlayerInput : IServiceTick, IDisposable
{
    private InputDriver driver = new();
    private InputRouter router = new();

    public PlayerInput() => Service.Register(this);

    public void Initialize()
    {
        driver.Initialize();
        router.Initialize();
    }

    public void Tick()
    {
        router.Tick();
    }

    public void Dispose()
    {
        driver.Dispose();
        router.Dispose();
    }

    public GamePhase Phase  => GamePhase.Input;
    public HashSet<InputButton> ActiveInput => router.ActiveButtons;
}

public class InputDriver : IDisposable
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

    void OnDodgePress      (InputAction.CallbackContext context) => EventBus<DodgePress>        .Raise(new());
    void OnDodgeRelease    (InputAction.CallbackContext context) => EventBus<DodgeRelease>      .Raise(new());

    void OnMovementVector  (InputAction.CallbackContext context) => EventBus<MovementVector>    .Raise(new MovementVector(context.ReadValue<Vector2>()));
    void OnMousePosition   (InputAction.CallbackContext context) => EventBus<MousePosition>     .Raise(new MousePosition (context.ReadValue<Vector2>()));


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

        input.Player.Move.performed     += OnMovementVector;
        input.Player.Move.canceled      += OnMovementVector;

        input.Player.Mouse.performed    += OnMousePosition;
        input.Player.Mouse.canceled     += OnMousePosition;
    }

    public void Dispose()
    {
        input?.Player.Disable();
        input?.Disable();
        input?.Dispose();
        input = null;
    }
}


public class InputRouter : IDisposable
{

    private Dictionary<InputIntent, InputButton> buttonMap   = new();
    private HashSet<InputButton> activeButtons               = new();

    private Vector2 mousePositionVector;
    private Vector2 movementDirectionVector;

    private EventBinding<InteractPress>         interactPress;
    private EventBinding<InteractRelease>       interactRelease;
    private EventBinding<ActionPress>           actionPress;
    private EventBinding<ActionRelease>         actionRelease;
    private EventBinding<Attack1Press>          attack1Press;
    private EventBinding<Attack1Release>        attack1Release;
    private EventBinding<Attack2Press>          attack2Press;
    private EventBinding<Attack2Release>        attack2Release;
    private EventBinding<ModifierPress>         modifierPress;
    private EventBinding<ModifierRelease>       modifierRelease;
    private EventBinding<DodgePress>            dodgePress;
    private EventBinding<DodgeRelease>          dodgeRelease;
    private EventBinding<MovementVector>        movementVector;
    private EventBinding<MousePosition>         mousePosition;

    public void Initialize()
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
        dodgePress      = BindPress     <DodgePress>        ();
        dodgeRelease    = BindRelease   <DodgeRelease>      ();
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
        EventBus<DodgePress>        .Unsubscribe(dodgePress);  
        EventBus<DodgeRelease>      .Unsubscribe(dodgeRelease);  
    }

    public void Tick()
    {
        UpdateButtons();
    }

    private InputButton GetOrCreateButton(InputIntent input)
    {
        if (!buttonMap.TryGetValue(input, out InputButton button))
        {
            button = new InputButton(input);
            buttonMap[input]    = button;
        }
        return button;
    }

    void UpdateButtons()
    {
        foreach (var button in buttonMap.Values)
        {
            UpdateAndDebugLog(button);

            if (button.Condition != InputCondition.None)
                activeButtons.Add(button);
            else
                activeButtons.Remove(button);
        }
    }

    void UpdateAndDebugLog(InputButton button)
    {
        button.Update();
        Logwin.Log("Button input", button.Input);
        Logwin.Log("Button condition", button.Condition);
    }

    void UpdateMousePosition(MousePosition evt)     => mousePositionVector      = evt.vector;
    void UpdateMovementIntent(MovementVector evt)   => movementDirectionVector  = evt.vector;

    EventBinding<T> BindPress<T>()   where T : IInputEvent => EventBus<T>.Subscribe(evt => { GetOrCreateButton(evt.Intent).Press();});
    EventBinding<T> BindRelease<T>() where T : IInputEvent => EventBus<T>.Subscribe(evt => { GetOrCreateButton(evt.Intent).Release();});

    public Vector2 MousePosition                => mousePositionVector;
    public Vector2 MovementDirection            => movementDirectionVector;
    public HashSet<InputButton> ActiveButtons   => activeButtons;
}

public enum InputIntent
{
    None,
    Interact,
    Action,
    Attack1,
    Attack2,
    Modifier,
    Dodge,
}

public enum InputCondition
{
    None,
    PressedThisFrame,
    Pressed,
    Held,
    ReleasedThisFrame,
    ReleasedRecently,
}

public class InputButton
{
    public InputIntent    Input            { get; }
    public InputCondition Condition
    {
        get
        {
            if (pressedThisFrame)           return InputCondition.PressedThisFrame;
            if (pressed)
                if (frameCountPressed.CurrentFrame > GameSettings.INPUT_THRESHOLD_HOLD)
                                            return InputCondition.Held;
                else
                                            return InputCondition.Pressed;
            if (releasedThisFrame)          return InputCondition.ReleasedThisFrame;
            if (frameCountReleased.CurrentFrame < GameSettings.INPUT_THRESHOLD_EXPIRY_RECENTLY_RELEASED)
                                            return InputCondition.ReleasedRecently;
                                            return InputCondition.None;
        }
    }

    public bool pressed                     = false;
    public bool pressedThisFrame            = false;
    public bool releasedThisFrame           = false;

    public  FrameCount frameCountPressed    = new();
    public  FrameCount frameCountReleased   = new();

    public InputButton(InputIntent input)   => Input = input;

    public void Update()
    {
        if (pressedThisFrame && frameCountPressed.CurrentFrame != frameCountPressed.StartFrame)
        {
            pressedThisFrame = false;
            pressed          = true;
        }

        if (releasedThisFrame && frameCountReleased.CurrentFrame != frameCountReleased.StartFrame)
        {
            releasedThisFrame = false;
        }

        if (Condition == InputCondition.None)
        {
            frameCountReleased.Stop();
        }
    }

    public void Press()
    {
        pressedThisFrame = true;
        frameCountPressed.Reset();
        frameCountPressed.Start();
    }

    public void Release()
    {
        pressed = false;
        frameCountPressed.Stop();
        releasedThisFrame = true;
        frameCountReleased.Reset();
        frameCountReleased.Start();
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

public struct DodgePress            : IInputEvent { public readonly InputIntent Intent => InputIntent.Dodge;    public readonly InputCondition Condition => InputCondition.PressedThisFrame;  }
public struct DodgeRelease          : IInputEvent { public readonly InputIntent Intent => InputIntent.Dodge;    public readonly InputCondition Condition => InputCondition.ReleasedThisFrame; }

public struct MousePosition         : IInputEvent { public readonly InputIntent Intent => InputIntent.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MousePosition(Vector2 value) => vector = value;   }
public struct MovementVector        : IInputEvent { public readonly InputIntent Intent => InputIntent.None;     public readonly InputCondition Condition => InputCondition.None; public Vector2 vector; public MovementVector(Vector2 value) => vector = value;  }

