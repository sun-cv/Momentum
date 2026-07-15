using System.Collections.Generic;
using System.Linq;



public class AbilitySystem : ActorService, IServiceTick
{

    readonly AbilityLoadout                                     loadout;
    readonly List<AbilityInstance>                              abilities;

        // -----------------------------------

    readonly AbilityCooldownSystem                              cooldown;

        // -----------------------------------

    Dictionary<AbilityPhase, IAbilityHandler>                   handlers;

    IReadOnlyDictionary<Trigger, Command>                       activeBuffer;
    IReadOnlyDictionary<Trigger, Command>                       inputBuffer;

    // ===============================================================================

    public AbilitySystem(Agent agent) : base(agent)
    {
        loadout         = new();
        abilities       = new();
        cooldown        = new();

        activeBuffer    = new Dictionary<Trigger, Command>();
        inputBuffer     = new Dictionary<Trigger, Command>();

        owner.Bus.Link.Local<EquipmentChangeEvent> (HandleEquipmentChange);
        owner.Bus.Link.Local<CommandPipelinesEvent>(HandleCommandPipelineUpdates);

        InitializePhaseHandlers();
    }


    void InitializePhaseHandlers()
    {
        handlers = new ()
        {
            { AbilityPhase.Enable,      new AbilityEnableHandler()  },
            { AbilityPhase.Charging,    new AbilityChargingHandler()},
            { AbilityPhase.Fire,        new AbilityFireHandler()    },
            { AbilityPhase.FireEnd,     new AbilityFireEndHandler() },
            { AbilityPhase.Disable,     new AbilityDisableHandler() },
            { AbilityPhase.Release,     new AbilityReleaseHandler() }
        };
    }

    public void Tick()
    {
        AdvanceAbilities();
        ProcessAbilities();
        ReleaseAbilities();

        DebugLog();
    }

    // ===================================
    //  Advance State
    // ===================================


    void AdvanceAbilities()
    {
        foreach(var instance in abilities )
        {
            if (instance is null)
                continue;

            AdvanceAbility(instance); 
        }
    }

    void AdvanceAbility(AbilityInstance instance)
    {
        if (ShouldRelease(instance))
        {
            DeactivateAbility(instance);
            return;
        }
        
        if (ShouldTerminate(instance))
        {
            switch(instance.Phase)
            {
                case AbilityPhase.Enable:   TransitionTo(instance, AbilityPhase.Disable); break;
                case AbilityPhase.Charging: TransitionTo(instance, AbilityPhase.Disable); break;
                case AbilityPhase.Fire:     TransitionTo(instance, AbilityPhase.FireEnd); break;
            }
            return;
        }

        handlers[instance.Phase].Update(this, instance);
    }


    // ===================================
    //  Ability Activation
    // ===================================

    void ProcessAbilities()
    {
        foreach (var (_, command) in inputBuffer.Concat(activeBuffer))
        {
            if (!ResolveAbility(command, out var ability, out var parent)) 
                continue;

            if (!CanActivate(ability))                                     
                continue;

            if (!Validate(ability, out var ended))                         
                continue;

            Commit(command, ability, parent, ended);
        }
    }


    bool ResolveAbility(Command command, out Ability ability, out AbilityInstance parent)
    {
        var held = activeBuffer.ContainsKey(command.Trigger);

        foreach (var instance in abilities)
        {
            foreach (var name in instance.ComboAbilities)
            {
                var combo = loadout.Abilities[name];

                if (combo.Lifecycle.ActivatesFromHeldCommand != held)
                    continue;

                if (!combo.Lifecycle.Triggers.Contains(command.Trigger))
                    continue;

                ability = combo;
                parent  = instance;
                return true;
            }
        }

        if (!held && loadout.Bindings.TryGetValue(command.Trigger, out var binding))
        {
            ability = loadout.Abilities[binding];
            parent  = null;
            return true;
        }

        ability = null;
        parent  = null;
        return false;
    }


    bool CanActivate(Ability ability)
    {
        var blockingPhase = new List<AbilityPhase>() { AbilityPhase.Charging, AbilityPhase.Fire, AbilityPhase.FireEnd };

        if (cooldown.IsOnCooldown(ability.Name))
            return false;

        if (ability.Lifecycle.ActivatesFromHeldCommand)
            foreach (var sustain in ability.Lifecycle.SustainTriggers)
                if (!activeBuffer.ContainsKey(sustain))
                    return false;

        if (abilities.Where(instance => blockingPhase.Contains(instance.Phase)).Any(instance => instance.Ability.Control.BlockedControl.Contains(ability.Name)))
            return false;

        return true;
    }


    bool Validate(Ability ability, out List<AbilityInstance> ended)
    {
        ended = new();

        var tag = ability.Permission.Tag;

        if (tag == AbilityTag.Instant)
            return true;

        foreach (var instance in abilities)
        {
            if (!instance.Ability.Permission.Phase.TryGetValue(instance.Phase, out var entry))
                continue;

            if (entry.CoexistWith.Contains(tag)) 
                continue;

            if (entry.CancelableBy.Contains(tag) && instance.Cancelable)
            {
                ended.Add(instance);
                continue;
            }

            return false;
        }

        return true;
    }

    void Commit(Command command, Ability ability, AbilityInstance parent, List<AbilityInstance> ended)
    {
        if (parent is not null)
            CancelAbility(parent);

        foreach (var instance in ended)
            CancelAbility(instance);

        var created = CreateAbility(ability, command.Intent);

        ActivateAbility(created);

        if (!ability.Lifecycle.ActivatesFromHeldCommand)
            ConsumeCommand(command);
    }

    AbilityInstance CreateAbility(Ability ability, IntentSnapshot intent)
    {
        return new(ability) { Intent = intent };
    }

    void CancelAbility(AbilityInstance instance)
    {
        instance.Canceled = true;
        TransitionTo(instance, AbilityPhase.Disable);
    }

    void ConsumeCommand(Command command)
    {
        owner.Bus.Emit.Local<Request, CommandAPI>(new() { Request = Request.Consume, Command = command });
    }


        // ===================================
        //  Ability Lifecycle
        // ===================================

    public void TransitionTo(AbilityInstance instance, AbilityPhase phase)
    {
        handlers[instance.Phase].Exit (this, instance);
        instance.Phase = phase; 
        handlers[instance.Phase].Enter(this, instance);
    }

    void ActivateAbility(AbilityInstance instance)
    {
        abilities.Add(instance); 
        SendAbilityActivation(instance);

        handlers[instance.Phase].Enter(this, instance);
    }

    void DeactivateAbility(AbilityInstance instance)
    {
        SendAbilityDeactivation(instance);

        abilities.Remove(instance);
    }

    public void RegisterCooldown(AbilityInstance instance)
    {
        if (instance.Ability.Timing.Cooldown.Count == 0)
            return;

        cooldown.RegisterAbilities(instance.Ability);
    }

    // ============================================================================
    //  Ability Release
    // ============================================================================

    void ReleaseAbilities()
    {
        foreach (var instance in abilities.Where(instance => instance.Phase == AbilityPhase.Release).ToList())
            DeactivateAbility(instance);
    }

    // ============================================================================
    //  Phase Management
    // ============================================================================

    public void SendPhaseRequests(AbilityInstance instance)
    {
        RequestMovement (instance);
        RequestFacing   (instance);
        RequestHitboxes (instance);
        RequestAnimation(instance);
    }

    public void ClearPhaseRequests(AbilityInstance instance)
    {
        ClearMovement   (instance);
        ClearFacing     (instance);
        ClearHitboxes   (instance);
        ClearAnimation  (instance);
    }

    public void TeardownAbility(AbilityInstance instance)
    {
        TeardownMovement(instance);
        TeardownFacing  (instance);
        TeardownHitboxes(instance);
        TeardownAnimations(instance);
    }

    // ============================================================================
    //  Movement Management
    // ============================================================================

    void RequestMovement(AbilityInstance instance)
    {
        foreach (var definition in instance.Ability.Movement.Actions)
        {
            if (definition.EnterPhase != instance.Phase)
                continue;

            owner.Bus.Emit.Local(new MovementEvent(instance, definition, instance.Intent));
        }
    }

    public void ClearMovement(AbilityInstance instance)
    {
        foreach (var definition in instance.Ability.Movement.Actions)
        {
            if (definition.ClearPhase != instance.Phase)
                continue;

            owner.Bus.Emit.Local(new ClearMovementEvent(instance, definition));
        }
    }

    public void TeardownMovement(AbilityInstance instance)
    {
        foreach (var definition in instance.Ability.Movement.Actions)
        {
            owner.Bus.Emit.Local(new ClearMovementEvent(instance, definition));
        }
    }

    // ============================================================================
    //  Facing Management
    // ============================================================================

    void RequestFacing(AbilityInstance instance)
    {

        if (instance.Ability.Facing.EnterPhase != instance.Phase)
            return;

        var Facing = new FacingAPI(instance.Ability.Facing)
        {
            Request     = Request.Claim,
            Claimant    = instance.RuntimeId, 
            Snapshot    = instance.Intent,
        };

        owner.Bus.Emit.Local<Request, FacingAPI>(Facing);
    }

    public void ClearFacing(AbilityInstance instance)
    {
        if (instance.Ability.Facing.ClearPhase != instance.Phase)
            return;

        var Facing = new FacingAPI(instance.Ability.Facing)
        {
            Request     = Request.Release,
            Claimant    = instance.RuntimeId, 
            Snapshot    = instance.Intent,
        };

        owner.Bus.Emit.Local<Request, FacingAPI>(Facing);
    }

    public void TeardownFacing(AbilityInstance instance)
    {
        var Facing = new FacingAPI(instance.Ability.Facing)
        {
            Request     = Request.Release,
            Claimant    = instance.RuntimeId, 
        };

        owner.Bus.Emit.Local<Request, FacingAPI>(Facing);
    }

    // ============================================================================
    //  Hitbox Management
    // ============================================================================

    void RequestHitboxes(AbilityInstance instance)
    {
        foreach (var hitboxDefinition in instance.Ability.Hitboxes)
        {            
            if (hitboxDefinition.Lifetime.EnterPhase != instance.Phase)
                continue;

            var API = new HitboxAPI(owner, hitboxDefinition)
            {
                Request     = Request.Create,
                Intent      = instance.Intent,
                Packages    = new List<object>
                {
                    new DamagePackage(instance.Ability.Damage.Components.ToList()),
                    new ForcePackage (instance.Ability.Damage.Forces.ToList())
                }
            };

            instance.Hitboxes.Add(API);

            Emit.Global<Request, HitboxAPI>(API);
        }
    }

    void ClearHitboxes(AbilityInstance instance)
    {
        var remove = new List<HitboxAPI>();

        foreach (var API in instance.Hitboxes)
        {
            if (API.Definition.Lifetime.ClearPhase != instance.Phase)
                continue;

            API.Request = Request.Destroy; 
            Emit.Global<Request, HitboxAPI>(API);

            remove.Add(API);
        }

        remove.ForEach(API => instance.Hitboxes.Remove(API));
    }

    void TeardownHitboxes(AbilityInstance instance)
    {
        foreach (var API in instance.Hitboxes)
        {
            API.Request = Request.Destroy; 
            Emit.Global<Request, HitboxAPI>(API);
        }
    }
    // ============================================================================
    // Animation System
    // ============================================================================

    void RequestAnimation(AbilityInstance instance)
    {
        foreach (var animation in instance.Ability.Animation.Entries)
        {
            if (animation.EnterPhase != instance.Phase)
                continue;

            var API = new AnimationAPI(animation.Animation)
            {
                Request = Request.Play,
            };

            if (animation.RequireManualRelease)
                API.Settings.HoldUntilReleased = true;

            // rework required - Removed once aim intent system is built

            if (animation.LockAimDuringPlayback)
                API.Data.Overrides.AddRange(AnimatorParameter.InputIntentSnapshot[typeof(IAimable)](instance.Intent));

            owner.Bus.Emit.Local<Request, AnimationAPI>(API);

            instance.Animations.Add(API);
        }
    }

    void ClearAnimation(AbilityInstance instance)
    {
        foreach (var animation in instance.Ability.Animation.Entries)
        {
            if (animation.ClearPhase != instance.Phase)
                continue;

            var API     = instance.Animations.First(instance => instance.Data.Animation == animation.Animation);
            API.Request = Request.Stop;

            owner.Bus.Emit.Local<Request, AnimationAPI>(API);
        }
    }

    void TeardownAnimations(AbilityInstance instance)
    {
        foreach (var animation in instance.Ability.Animation.Entries)
        {
            var API     = instance.Animations.First(instance => instance.Data.Animation == animation.Animation);
            API.Request = Request.Stop;

            owner.Bus.Emit.Local<Request, AnimationAPI>(API);
        }
    }

    // ============================================================================
    // Control System
    // ============================================================================

    public void UpdateAvailableControls(AbilityInstance instance)
    {
        switch (instance.Phase)
        {
            case AbilityPhase.Charging:
                AddControls(instance, instance.Ability.Control.AddControlOnCharge);
                RemoveControls(instance, instance.Ability.Control.RemoveControlOnCharge);
                break;

            case AbilityPhase.Fire:
                AddControls(instance, instance.Ability.Control.AddControlOnFire);
                RemoveControls(instance, instance.Ability.Control.RemoveControlOnFire);
                break;

            case AbilityPhase.FireEnd:
                AddControls(instance, instance.Ability.Control.AddControlOnFireEnd);
                RemoveControls(instance, instance.Ability.Control.RemoveControlOnFireEnd);

                break;
        }

        if (instance.ComboAbilities.Count > 0)
            Log.Trace("Control.Available", () => $"{string.Join(", ", instance.ComboAbilities)}");
    }

    public void ClearAvailableControls(AbilityInstance instance)
    {
        var control     = instance.Ability.Control;
        var controls    = control.AddControlOnCharge.Concat(control.AddControlOnFire).Concat(control.AddControlOnFireEnd).ToList();

        RemoveControls(instance, controls);
    }

    void AddControls(AbilityInstance instance, List<string> controls)
    {
        if (controls == null)
            return;

        foreach (var control in controls)
            instance.ComboAbilities.Add(control);
    }

    void RemoveControls(AbilityInstance instance, List<string> controls)
    {
        if (controls == null)
            return;

        foreach (var control in controls)
            instance.ComboAbilities.Remove(control);
    }
    // ============================================================================
    //  Events
    // ============================================================================

    void HandleCommandPipelineUpdates(CommandPipelinesEvent message)
    {
        activeBuffer    = message.Active;
        inputBuffer     = message.Buffer;
    }

    void HandleEquipmentChange(EquipmentChangeEvent message)
    {

        if (message.Equipment is not Weapon weapon)
            return;

        switch(message.Type)
        {
            case Publish.Equipped:
                    loadout.AddSet(weapon.AbilitySet);
                break;
                
            case Publish.Unequipped:
                    loadout.RemoveSet(weapon.AbilitySet);
                break;
        }
    }

        // ===================================
        //  Emitters
        // ===================================

    void SendAbilityActivation(AbilityInstance instance)
    {
        owner.Bus.Emit.Local(new AbilityEvent(Publish.Activated, owner, instance));
    }

    void SendAbilityDeactivation(AbilityInstance instance)
    {
        owner.Bus.Emit.Local(new AbilityEvent(Publish.Deactivated, owner, instance));
    }

    // ============================================================================
    //  Predicates
    // ============================================================================

    bool ShouldRelease(AbilityInstance instance)
    {
        return instance.Phase == AbilityPhase.Release;
    }

    bool ShouldTerminate(AbilityInstance instance)
    {
        if (instance.Ability.Lifecycle.SustainTriggers.Any(trigger => !IsTriggerActive(trigger)))
            return true;

        return false;
    }

    public bool IsTriggerActive(Trigger trigger)
    {
        return activeBuffer.ContainsKey(trigger) || inputBuffer.ContainsKey(trigger);
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Ability);

    void DebugLog()
    {
        foreach (var instance in abilities)
        {
            Log.Debug("Ability.Active",         () => instance.Ability.Name ?? "none");
            Log.Debug("Ability.Phase",          () => instance.Phase.ToString() ?? "none");
            Log.Trace("Commands.Active",        () => activeBuffer?.Count > 0 ? string.Join(", ", activeBuffer?.Keys) : "");
            Log.Trace("Commands.Buffered",      () => inputBuffer?.Count > 0 ? string.Join(", ", inputBuffer?.Keys) : "");
            Log.Trace("Cooldown",               () => cooldown.IsOnCooldown(instance.Ability.Name) ?  $"Remaining: {cooldown.GetRemainingCooldown(instance.Ability.Name)}" : "Ready");
        }

        Log.Trace("Ability.Instances",          () => abilities.Count);
        // if (owner is Hero hero)
        // {
        //     if (instance != null && instance.State.Phase == WeaponPhase.Enable)
        //     {
        //         Log.Debug( $" {instance.State.Phase} {hero.Effects.Can<IDisableRotate>(effect => effect.DisableRotate, defaultValue: !hero.Disabled)}");
        //         Log.Debug( $" {instance.State.Phase} Disabled: {hero.Disabled}");
        //         Log.Debug( $" {instance.State.Phase} Locked aim x {hero.ResolvedAim.Cardinal.x}");
        //         Log.Debug( $" {instance.State.Phase} Locked aim y {hero.ResolvedAim.Cardinal.y}");
        //     }
        //
        //     if (instance != null && instance.State.Phase == WeaponPhase.Charging)
        //     {
        //
        //         foreach (var effect in hero.Effects.Effects)
        //         {
        //             Log.Debug($"Active effect: {effect.Effect.Name} | DisableRotate: {(effect.Effect is IDisableRotate r ? r.DisableRotate : false)}");
        //         }
        //
        //         Log.Debug( $" {instance.State.Phase} {hero.Effects.Can<IDisableRotate>(effect => effect.DisableRotate, defaultValue: !hero.Disabled)}");
        //         Log.Debug( $" {instance.State.Phase} Disabled: {hero.Disabled}");
        //         Log.Debug( $" {instance.State.Phase} Locked aim x {hero.ResolvedAim.Cardinal.x}");
        //         Log.Debug( $" {instance.State.Phase} Locked aim y {hero.ResolvedAim.Cardinal.y}");
        //     }
        // }
    }
    
    public AbilityCooldownSystem Cooldown => cooldown;

    public UpdatePriority Priority => ServiceUpdatePriority.WeaponLogic;

}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct AbilityEvent : IMessage
{
    public readonly Actor Owner                 { get; init; }
    public readonly AbilityInstance Instance    { get; init; }
    public readonly Publish Type                { get; init; }

    public AbilityEvent(Publish type, Actor owner, AbilityInstance instance)
    {
        Owner       = owner;
        Instance    = instance;
        Type        = type;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Handlers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IAbilityHandler 
{
    void Enter  (AbilitySystem system, AbilityInstance instance);
    void Update (AbilitySystem system, AbilityInstance instance);
    void Exit   (AbilitySystem system, AbilityInstance instance);
    AbilityPhase Phase { get; }
}


    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //  Enable
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AbilityEnableHandler : IAbilityHandler
{
    public void Enter   (AbilitySystem system, AbilityInstance instance)
    {
        system.TransitionTo(instance, AbilityPhase.Charging);
    }

    public void Update  (AbilitySystem system, AbilityInstance instance) {}
    public void Exit    (AbilitySystem system, AbilityInstance instance) {}

    public AbilityPhase Phase => AbilityPhase.Enable;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //  Charging
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AbilityChargingHandler : IAbilityHandler
{
    public void Enter   (AbilitySystem system, AbilityInstance instance)
    {        
        instance.FrameCount.Restart();

        system.UpdateAvailableControls(instance);
        system.SendPhaseRequests(instance);
    }

    public void Update(AbilitySystem system, AbilityInstance instance)
    {
        HandleCancelWindow(instance);

        if (ResolveAbilityActivation(system, instance))
        {
            system.TransitionTo(instance, AbilityPhase.Fire); 
            return;
        }

        if (ResolveAbilityRelease(system, instance))
        {
            system.TransitionTo(instance, AbilityPhase.Disable);
            return;
        }
    }

    public void Exit(AbilitySystem system, AbilityInstance instance)
    {
        instance.Cancelable = false;
        system.ClearPhaseRequests(instance);
    }

    // ===============================================================================

    void HandleCancelWindow(AbilityInstance instance)
    {
        if (instance.Cancelable)
            return;

        if (instance.Ability.Permission.Phase.TryGetValue(AbilityPhase.Charging, out var permission) && permission.CancelableBy.Count == 0)
            return;

        if (instance.Ability.Timing.Phase.TryGetValue(AbilityPhase.Charging, out var timing) && timing.CancelFrameOffset >= instance.FrameCount.CurrentFrame)
            return;

        instance.Cancelable = true;
    }

    bool ResolveAbilityActivation(AbilitySystem system, AbilityInstance instance)
    {
        switch(instance.Ability.Lifecycle.Activation)
        {
            case AbilityActivation.OnPress:
            case AbilityActivation.OnCharge:
            case AbilityActivation.WhileHeld:
                if (ChargeComplete(instance)) return true; break;
            case AbilityActivation.OnRelease: 
                if ((ChargeComplete(instance) && instance.Ability.Control.ForceRelease) || (MinimumChargeComplete(instance) && TriggerReleased(system, instance))) return true; break;
        }

        return false; 
    }

    bool ResolveAbilityRelease(AbilitySystem system, AbilityInstance instance)
    {
        switch(instance.Ability.Lifecycle.Activation)
        {
            case AbilityActivation.OnPress:
                return false;
            case AbilityActivation.OnCharge:
            case AbilityActivation.WhileHeld:
                if (!ChargeComplete(instance) && TriggerReleased(system, instance)) return true; break;
            case AbilityActivation.OnRelease: 
                if (!MinimumChargeComplete(instance) && TriggerReleased(system, instance)) return true; break;
        }

        return false; 
    }
        
    // ===============================================================================

    bool ChargeComplete(AbilityInstance instance)
    {
        return instance.FrameCount.CurrentFrame >= instance.Ability.Timing.Phase[AbilityPhase.Charging].Frames;
    }

    bool MinimumChargeComplete(AbilityInstance instance)
    {
        if (instance.FrameCount.CurrentFrame >= instance.Ability.Timing.Phase[AbilityPhase.Charging].Minimum) 
            return true;

        return false;
    }

    bool TriggerReleased(AbilitySystem system, AbilityInstance instance)
    {
        if(instance.Ability.Lifecycle.Triggers.Any(trigger => !system.IsTriggerActive(trigger)))
            return true;

        return false;
    }

    public AbilityPhase Phase => AbilityPhase.Charging;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //  Fire
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AbilityFireHandler : IAbilityHandler
{
    public void Enter(AbilitySystem system, AbilityInstance instance)
    {        
        instance.FrameCount.Restart();

        system.UpdateAvailableControls(instance);
        system.SendPhaseRequests(instance);

        HandleCancelWindow(instance);
    }

    public void Update(AbilitySystem system, AbilityInstance instance)
    {
        HandleCancelWindow(instance);

        if (ResolveAbilityRelease(system, instance))
            system.TransitionTo(instance, AbilityPhase.FireEnd);
    }

    public void Exit(AbilitySystem system, AbilityInstance instance)
    {
        system.RegisterCooldown(instance);
        system.ClearPhaseRequests(instance);
    }

    // ===============================================================================

    void HandleCancelWindow(AbilityInstance instance)
    {
        if (instance.Ability.Permission.Phase[AbilityPhase.Fire].CancelableBy.Count == 0)
            return;

        if (instance.Ability.Timing.Phase[AbilityPhase.Fire].CancelFrameOffset <= instance.FrameCount.CurrentFrame)
            instance.Cancelable = true;
    }


    bool ResolveAbilityRelease(AbilitySystem system, AbilityInstance instance)
    {
        if (FireComplete(instance)) 
            return true;

        if (instance.Ability.Lifecycle.Activation == AbilityActivation.WhileHeld && TriggerReleased(system, instance)) 
            return true;

        return false; 
    }

    // ===============================================================================

    bool TriggerReleased(AbilitySystem system, AbilityInstance instance)
    {
        if(instance.Ability.Lifecycle.Triggers.Any(trigger => !system.IsTriggerActive(trigger)))
            return true;

        return false;
    }

    bool FireComplete(AbilityInstance instance)
    {
        return instance.FrameCount.CurrentFrame >= instance.Ability.Timing.Phase[AbilityPhase.Fire].Frames;
    }

    public AbilityPhase Phase => AbilityPhase.Fire;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //  FireEnd
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AbilityFireEndHandler : IAbilityHandler
{
    public void Enter(AbilitySystem system, AbilityInstance instance)
    {        
        instance.FrameCount.Restart();

        system.UpdateAvailableControls(instance);
        system.SendPhaseRequests(instance);
    }

    public void Update(AbilitySystem system, AbilityInstance instance)
    {
        system.TransitionTo(instance, AbilityPhase.Disable);
    }

    public void Exit(AbilitySystem system, AbilityInstance instance)
    {
        system.ClearPhaseRequests(instance);
    }

    public AbilityPhase Phase => AbilityPhase.FireEnd;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //  Disable
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AbilityDisableHandler : IAbilityHandler
{
    public void Enter(AbilitySystem system, AbilityInstance instance)
    {        
        instance.FrameCount.Restart();

        ProcessControlWindow(instance);
    }

    public void Update(AbilitySystem system, AbilityInstance instance)
    {
        if (ControlWindowPending(instance))
            return;
            
        system.TransitionTo(instance, AbilityPhase.Release);
    }

    public void Exit(AbilitySystem system, AbilityInstance instance)
    {
        system.ClearAvailableControls(instance);
        system.TeardownAbility(instance);
    }

    // ===============================================================================

    void ProcessControlWindow(AbilityInstance instance)
    {
        if (instance.Canceled)
            return;

        if (instance.Ability.Timing.ControlWindow > 0)
        {
            instance.ComboControlWindow = new FrameTimer(instance.Ability.Timing.ControlWindow);
            instance.ComboControlWindow.Start();
        }
    }

    bool ControlWindowPending(AbilityInstance instance)
    {
        if (instance.ComboControlWindow != null)
            return instance.ComboControlWindow.IsRunning;

        return false;
    }

    public AbilityPhase Phase => AbilityPhase.Disable;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //  Release
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AbilityReleaseHandler : IAbilityHandler
{
    public void Enter   (AbilitySystem system, AbilityInstance instance) {}
    public void Update  (AbilitySystem system, AbilityInstance instance) {}
    public void Exit    (AbilitySystem system, AbilityInstance instance) {}

    public AbilityPhase Phase => AbilityPhase.Release;
}
