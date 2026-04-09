using UnityEngine;



public class Lifecycle : ActorService, IServiceLoop
{
    public enum State { Alive, Dying, Dead, Disposal }

    // ===============================================================================
    
    LifecycleStateMachine stateMachine; 

        // -----------------------------------

    LifecycleContext context;

    // ===============================================================================

    public Lifecycle(Actor actor) : base(actor)
    {
        if (actor is not IMortal)
            return;

        stateMachine = new(this);

        CreateContext();
        InitializeState();

        Enable();
    }

    void InitializeState()
    {
        stateMachine = new(this);

        stateMachine.Register(State.Alive,    new LifecycleAliveState(stateMachine));
        stateMachine.Register(State.Dying,    new LifecycleDyingState(stateMachine));
        stateMachine.Register(State.Dead,     new LifecycleDeadState (stateMachine));

        stateMachine.Initialize(State.Alive);
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessState();
    }

    // ===============================================================================

    void ProcessState()
    {
        stateMachine.Update();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    public void PublishState()
    {
        owner.Bus.Emit.Local(new LifecycleEvent(owner, stateMachine.State));
    }
    
    // ===============================================================================

    void CreateContext()
    {
        context = new();
        context.Corpse.Actor = owner;
    }

    // ===============================================================================

    // readonly Logger Log = Logging.For(LogSystem.Lifecycle);

    public IMortal Mortal               => owner as IMortal;
    public bool IsAlive                 => stateMachine.State == State.Alive;
    public bool IsDead                  => stateMachine.State == State.Dead;
    public LifecycleContext Context     => context;

    public UpdatePriority Priority      => ServiceUpdatePriority.Lifecycle;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Classes                                       

public class LifecycleContext
{
    public CorpseContext Corpse             { get; set; } = new();
    public bool KillingBlowReceived         { get; set; }

    public void Reset()
    {
        Corpse              = new();
        KillingBlowReceived = false;
    }
}

public class LifecycleStateMachine : StateMachine<Lifecycle.State>
{
    readonly Lifecycle controller;

    public LifecycleStateMachine(Lifecycle controller) : base(controller.PublishState) 
    {
        this.controller = controller;
    }

    public Lifecycle Controller => controller;
}

public class LifecycleState : MachineState<Lifecycle.State, LifecycleStateMachine>
{
    public LifecycleState(LifecycleStateMachine machine) : base(machine) {}
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Alive state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleAliveState : LifecycleState, IStateHandler
{
    readonly Actor owner;

    // ===============================================================================

    public LifecycleAliveState(LifecycleStateMachine machine) : base(machine)
    {
        owner       = machine.Controller.Owner;
        owner.Bus.Link.Local<KillingBlow>(HandleKillingBlow);
    }

    // ===============================================================================

    public void Enter()
    {

    }
    
    public void Update()
    {
        if (machine.Controller.Context.KillingBlowReceived)
            machine.TransitionTo(Lifecycle.State.Dying);
    }
    
    public void Exit()
    {
    }

    // ===============================================================================

    void HandleKillingBlow(KillingBlow message)
    {
        machine.Controller.Context.Corpse.KillingBlow   = message;
        machine.Controller.Context.KillingBlowReceived  = true;
    }

    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Alive;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Dying state                                       
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleDyingState : LifecycleState, IStateHandler
{
    readonly Actor              owner;
    readonly IMortal            mortal;
    readonly ActorDefinition    definition;

        // -----------------------------------

    readonly LocalEventHandler<Message<Response, AnimationAPI>> animationRequestHandler;

    // ===============================================================================

    public LifecycleDyingState(LifecycleStateMachine machine) : base(machine)
    {

        this.owner      = machine.Controller.Owner;
        this.mortal     = machine.Controller.Owner as IMortal;
        this.definition = machine.Controller.Owner.Definition;

        animationRequestHandler = new(owner.Bus, HandleAnimationAPI);

        owner.Bus.Link.Local<AnimationEvent>(HandleAnimatorEvent);
    }
    
    // ===============================================================================
    
    public void Enter()
    {
        DisableDamage();
        DisableControl();

        if (AlertOnDeathEnabled())
            AlertDeath();

        if (HasDeathAnimation())
            RequestDeathAnimation();

        if (HasOnDeathEffects())
            ApplyOnDeathEffects();
    }
    
    public void Update()
    {
        if (!HasDeathAnimation())
            machine.TransitionTo(Lifecycle.State.Dead);
    }
    
    public void Exit()
    {
        KillVelocity();
        RecordPosition();
    }
    
    // ===============================================================================

    void DisableControl()
    {
        if (owner is IControllable controllable)
            controllable.Inactive = true;
    }

    void DisableDamage()
    {
        mortal.Invulnerable = true;
    }

    void AlertDeath()
    {
        owner.Bus.Emit.Local(new ActorDeathEvent(owner));
    }

    void ApplyOnDeathEffects()
    {
        foreach (var effect in definition.Lifecycle.OnDeathEffects)
        {
            var API = new EffectAPI(owner, effect)
            {
                Request = Request.Create
            };

            owner.Bus.Emit.Local(API.Request, API);
        }
    }

    void KillVelocity()
    {
        if (owner is IMovableActor movable)
            movable.Velocity = Vector2.zero;
    }

    void RecordPosition()
    {
        machine.Controller.Context.Corpse.Position = owner.Bridge.View.transform.position;
    }

        // ===================================
        //  Animation
        // ===================================

    void RequestDeathAnimation()
    {
        var request = new AnimationAPI(AnimationIntent.Death) 
        {
            Request  = Request.Play,
            Settings = new() 
            { 
                AllowInterrupt      = false,
            },
            
        };

        if (CanBecomeCorpse())
            request.Settings.HoldOnPlaybackEnd = true;

        animationRequestHandler.Forward(request.Id, request.Request, request);
    }

    void HandleAnimationAPI(Message<Response, AnimationAPI> response)
    {
        machine.Controller.Context.Corpse.DeathAnimation = response.Payload;
    }

    void HandleAnimatorEvent(AnimationEvent message)
    {
        var animation = message.Animation;

        if (machine.Controller.Context.Corpse.DeathAnimation == null)
            return;
 
        if (animation.State != Animation.Playback.Held)
            return;

        if (machine.Controller.Context.Corpse.DeathAnimation.Data.Animation == animation.Name)
        {
            Debug.Log("Transition dead..");
            machine.TransitionTo(Lifecycle.State.Dead);
        }
    }


    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool HasDeathAnimation()
    {
        return definition.Appearance.Animations.Death.Enabled;
    }

    bool HasOnDeathEffects()
    {
        return definition.Lifecycle.OnDeathEffects?.Count > 0;
    }

    bool AlertOnDeathEnabled()
    {
        return definition.Lifecycle.AlertOnDeath;
    }

    bool CanBecomeCorpse()
    {
        return definition.Lifecycle.Spawn.Corpse != "";
    }
    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Dying;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Dead state                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleDeadState : LifecycleState, IStateHandler
{
    readonly Actor              owner;
    readonly ActorDefinition    definition;
    
    // ===============================================================================

    public LifecycleDeadState(LifecycleStateMachine machine) : base(machine)
    {
        this.owner      = machine.Controller.Owner;
        this.definition = machine.Controller.Owner.Definition;
    }
    
    // ===============================================================================

    public void Enter()
    {
        RequestAbsenceOrDisposal();
    }

    public void Update()
    {
        Exit();
    }

    public void Exit()
    {
        TrySpawnCorpse();
    }

    // ===============================================================================

    void TrySpawnCorpse()
    {
        if (!CanBecomeCorpse())
            return;

        RequestCorpseSpawn();
    }

    void RequestAbsenceOrDisposal()
    {
        switch(definition.Lifecycle.Respawn.Enabled)
        {   
            case true:  RequestAbsence();   break;
            case false: RequestDisposal();  break;
        }
    }
        
    void RequestCorpseSpawn()
    {
        Emit.Global(new CorpseRequest(machine.Controller.Context.Corpse));
    }

    void RequestAbsence()
    {
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Absent));
    }

    void RequestDisposal()
    {
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Disposal));
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool CanBecomeCorpse()
    {
        return definition.Lifecycle.Spawn.Corpse != "";
    }

    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Dead;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events                                         
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct LifecycleEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly Lifecycle.State State   { get; init; }

    public LifecycleEvent(Actor owner, Lifecycle.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

public readonly struct LifecycleTargetEvent : IMessage
{
    public readonly Lifecycle.State Target  { get; init; }

    public LifecycleTargetEvent(Lifecycle.State target)
    {
        Target = target;
    }
}

public readonly struct ActorDeathEvent : IMessage
{
    public readonly Actor Owner             { get; init; }

    public ActorDeathEvent(Actor owner)
    {
        Owner   = owner;
    }
}
