using System;
using System.Collections.Generic;
using UnityEngine;

public class InputRouter : RegisteredService, IServiceTick, IInitialize, IDisposable
{

    readonly Dictionary<Capability, InputButton> buttonMap      = new();
    readonly HashSet<InputButton> activeButtons                 = new();
    readonly Queue<PendingInputEvent> pendingInputs             = new();

    Vector2 mousePositionVector                                 = new();
    Vector2 movementDirectionVector                             = new();

        // -----------------------------------

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

    // ===============================================================================

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
        dashPress       = BindPress     <DashPress>         ();
        dashRelease     = BindRelease   <DashRelease>       ();

        foreach (Capability intent in EnumUtils.GetEnumValues<Capability>())
        {
            if (intent == Capability.None)
                continue;

            buttonMap[intent] = new InputButton(intent);
        }
    }

    // ===============================================================================
    
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
                GetOrCreateButton(evt.Action).Press();
            else
                GetOrCreateButton(evt.Action).Release();
        }

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

    InputButton GetOrCreateButton(Capability input)
    {
        if (!buttonMap.TryGetValue(input, out InputButton button))
        {
            button = new InputButton(input);
            buttonMap[input] = button;
        }

        return button;
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void UpdateMousePosition(MousePosition evt)             => mousePositionVector      = evt.vector;
    void UpdateMovementIntent(MovementVector evt)           => movementDirectionVector  = evt.vector;

    EventBinding<T> BindPress<T>()   where T : IInputEvent  => EventBus<T>.Subscribe(evt => { pendingInputs.Enqueue(new() { Action = evt.Action, IsPress = true  }); });
    EventBinding<T> BindRelease<T>() where T : IInputEvent  => EventBus<T>.Subscribe(evt => { pendingInputs.Enqueue(new() { Action = evt.Action, IsPress = false }); });

    // ===============================================================================


    public override void Dispose()
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
        EventBus<MovementVector>    .Unsubscribe(movementVector);  
        EventBus<MousePosition>     .Unsubscribe(mousePosition);  
    }

    public Vector2 MousePosition                            => mousePositionVector;
    public Vector2 MovementDirection                        => movementDirectionVector;

    public Dictionary<Capability, InputButton> ButtonMap    => buttonMap;
    public HashSet<InputButton> ActiveButtons               => activeButtons;
    
    public UpdatePriority Priority => ServiceUpdatePriority.InputRouter;
}


