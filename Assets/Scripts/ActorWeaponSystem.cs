using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// REWORK REQUIRED
// Bug found weapon system > weapon animation
// Reproduction:
// Start any attack animation
// Immediately after hold right click to queue shield parry/block
// Animation controller will not release action layer animation state


// CHECK REQUIRED
// Interrupt validation handler
// On held validation allows early true - may cause issues with can overwrite pending?

public class WeaponSystem : ActorService, IServiceTick
{
    readonly Agent                                          agent;
    readonly WeaponLoadout                                  loadout;

    // -----------------------------------

    readonly WeaponCooldown                                 cooldown;
    readonly WeaponActivationValidator                      validator;

    // -----------------------------------

    readonly GlobalEventHandler<Message<Response, HitboxAPI>> hitboxRequestHandler;

        // -----------------------------------

    Dictionary<WeaponPhase, IWeaponPhaseHandler>            phaseHandlers;
    Dictionary<WeaponActivation, IActivationStrategy>       activationStrategies;

        // -----------------------------------

    WeaponAction                                            pendingWeapon;
    
    WeaponInstance                                          instance;
    WeaponInstance                                          instanceStorage;

    IReadOnlyDictionary<Capability, Command>                activeBuffer;
    IReadOnlyDictionary<Capability, Command>                inputBuffer;
    IReadOnlyDictionary<Capability, IReadOnlyList<string>>  locks;

    public int NonCancelableAttackLocks                     { get; set; } = 0;
    public bool AimEnabled                                  { get; set; }

    // ===============================================================================

    public WeaponSystem(Agent agent) : base(agent)
    {
        this.agent           = agent;

        loadout         = new();
        cooldown        = new();
        validator       = new(this);
        
        hitboxRequestHandler       = new(HandleHitboxAPI);

        InitializePhaseHandlers();
        InitializeActivationStrategies();

        owner.Bus.Link.Local<CommandPipelinesEvent>(HandleCommandPipelineUpdates);
        owner.Bus.Link.Local<EffectEvent>          (HandleEffectNonCancelableLockCount);
        owner.Bus.Link.Local<LockUpdateEvent>      (HandleLocksUpdate);
        owner.Bus.Link.Local<EquipmentChangeEvent> (HandleEquipmentChange);

        AimEnabled = this.agent is IAimable;

        Enable();
    }

    void InitializePhaseHandlers()
    {
        phaseHandlers = new()
        {
            { WeaponPhase.Enable,           new EnablePhaseHandler()            },
            { WeaponPhase.Charging,         new ChargingPhaseHandler()          },
            { WeaponPhase.Fire,             new FirePhaseHandler()              },
            { WeaponPhase.FireEnd,          new FireEndPhaseHandler()           },
            { WeaponPhase.Disable,          new DisablePhaseHandler()           }
        };
    }

    void InitializeActivationStrategies()
    {
        activationStrategies = new()
        {
            { WeaponActivation.OnPress,     new OnPressActivationStrategy()     },
            { WeaponActivation.OnCharge,    new OnChargeActivationStrategy()    },
            { WeaponActivation.OnRelease,   new OnReleaseActivationStrategy()   },
            { WeaponActivation.WhileHeld,   new WhileHeldActivationStrategy()   }
        };
    }

    // ===============================================================================

    public void Tick()
    {
        AdvanceWeaponState();
        ProcessWeaponActivation();
        ExecuteWeaponActivation();

        ProcessWeaponServices();
    }


    // ===============================================================================

    // ===================================
    //  Advance State
    // ===================================

    void AdvanceWeaponState()
    {
        if (!HasActiveWeapon())
            return;

        if (ResolveWeaponRelease())
            return;
    
        if (ResolveWeaponDisable())
            return;

        if (ResolveWeaponActivationStrategies())
            return;
        
        UpdateWeaponState();
    }

    bool ResolveWeaponRelease()
    {
        if (!ShouldReleaseWeapon())
            return false;

        ReleaseWeapon();
        return true;
    }

    bool ResolveWeaponDisable()
    {
        if (!ShouldDisableWeapon())
            return false;
                     
        DisableWeapon();
        return true;
    }

    bool ResolveWeaponActivationStrategies()
    {
        var strategy = GetActivationStrategy();

        return instance.State.Phase switch
        {
            WeaponPhase.Charging    => strategy.ShouldTransitionOnFireRelease(instance, this),
            WeaponPhase.Fire        => strategy.ShouldTransitionOnChargeRelease(instance, this),
            _ => false
        };    
    }

    void UpdateWeaponState()
    {
        if (phaseHandlers.TryGetValue(instance.State.Phase, out var handler))
            handler.Update(this);
    }
        // ===================================
        //  Process Weapon Services
        // ===================================

    void ProcessWeaponServices()
    {
        ProcessAim();
        DebugLog();
    }
        // ===================================
        //  Process Weapon Activation
        // ===================================

    void ProcessWeaponActivation()
    {
        if (ResolveInterruptWeaponTransition())
            return;
       
        if (ResolveAvailableWeaponTransition())
            return;

        if (ResolveDefaultWeaponActivation())
            return;
    }

    bool ResolveInterruptWeaponTransition()
    {
        if (!HasInputBuffer() || !HasActiveWeapon())
            return false;

        foreach (var command in inputBuffer.Values)
        {
            var weapon = loadout.GetActionByType(command.Action, WeaponType.Interrupt);

            if (!CanActivateFromInterruptControls(weapon))
                continue;

            pendingWeapon = weapon;

            DisableWeapon();
            return true;
        }

        return false;
    }

    bool ResolveAvailableWeaponTransition()
    {
        if (!HasInputBuffer() || !HasActiveWeapon())
            return false;

        foreach (var control in instance.State.AvailableControls)
        {
            var weapon = loadout.GetAction(control);

            if (!CanActivateFromAvailableControls(weapon))
                continue;

            pendingWeapon = weapon;

            DisableWeapon();
            return true;
        }

        return false;
    }

    bool ResolveDefaultWeaponActivation()
    {
        if (!HasInputBuffer() || HasActiveWeapon())
            return false;

        foreach (var command in inputBuffer.Values)
        {
            var weapon = loadout.DefaultWeapon(command.Action);

            if (!CanActivateFromDefaultControls(weapon))
                continue;
            
            pendingWeapon = weapon;
            return true;
        }
        return false;
    }

        // ===================================
        //  Execute Weapon Activation
        // ===================================

    void ExecuteWeaponActivation()
    {
        if (HasActiveWeapon())
            return;

        if (!HasPendingWeapon())
            return;

        ActivateWeapon();
        EnableWeapon();
    }

    

    public void TransitionTo(WeaponPhase newPhase)
    {
        ExitHandler();
        TransitionState(newPhase);
        EnterHandler();
    }

    void EnterHandler()
    {
        if (phaseHandlers.TryGetValue(instance.State.Phase, out var handler))
            handler.Enter(this);
    }

    void TransitionState(WeaponPhase phase)
    {
        instance.State.Phase = phase;
        PublishTransition();
    }

    void ExitHandler()
    {
        if (phaseHandlers.TryGetValue(instance.State.Phase, out var handler))
            handler.Exit(this);
    }

        // ===================================
        //  Aim
        // ===================================

    void ProcessAim()
    {
        if (!AimEnabled || !HasActiveWeapon()) 
           return;

        WeaponAimProcessor.Process(instance, Owner, Time.deltaTime, out bool facingChanged);

        if (facingChanged)
            RequestFacingDirection();

        UpdateHitboxAngle();
    }


        // ===================================
        //  Equip and Enable
        // ===================================

    void ActivateWeapon()
    {
        CreateInstance(pendingWeapon);
        ResolveCommandActivation();
        ResolveWeaponAimActivation();
        SendWeaponEquip(pendingWeapon);

        ClearPendingWeapon();
    }

    void EnableWeapon()
    {
        EnterHandler();
    }

    void DisableWeapon()
    {
        TransitionTo(WeaponPhase.Disable);
    }

    void ReleaseWeapon()
    {
        StoreAndReleaseInstance();
        SendWeaponUnequip();
    }


        // ===================================
        //  Validation
        // ===================================

    bool HasAllRequiredTriggers(WeaponAction weapon)
    {
        bool hasAll = weapon.Trigger.All(trigger => activeBuffer.ContainsKey(trigger) || inputBuffer.ContainsKey(trigger));
        return hasAll;
    }

    bool HasNewCommandForWeapon(WeaponAction weapon)
    {
        return weapon.Trigger.Any(trigger => IsNewCommand(trigger));
    }

    bool IsNewCommand(Capability trigger)
    {
        if (inputBuffer.TryGetValue(trigger, out var command))
        {
            bool isNew = !instance.State.OwnedCommands.Contains(command.RuntimeId);
            return isNew;
        }
        return false;
    }

    public bool IsTriggerActive(Capability trigger)
    {
        return activeBuffer.ContainsKey(trigger) || inputBuffer.ContainsKey(trigger);
    }

    public bool OnlyCancelableLocksRemain() => NonCancelableAttackLocks == 0;


    // ============================================================================
    //  Command Management
    // ============================================================================

    void ResolveCommandActivation()
    {        
        switch (instance.Action.Availability)
        {
            case WeaponAvailability.Default:
                ConsumeAllCommands(inputBuffer, instance.Action.Trigger);
                break;
            case WeaponAvailability.OnPhase:
                ConsumeAllCommands(inputBuffer, instance.Action.Trigger);
                break;
            case WeaponAvailability.OnHeld:
                break;  
        }

        StoreAllCommandIDs      (activeBuffer, instance.Action.Trigger);
        StoreInputIntentSnapshot(activeBuffer, instance.Action.Trigger);

        if (instance.Action.LockTriggerAction)
            LockAllCommands(activeBuffer, instance.Action.Trigger);
    }

    void ConsumeCommand(Command command)
    {
        owner.Bus.Emit.Local(new CommandEvent(Request.Consume, command));
    }

    void ConsumeAllCommands(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var cmd))
                ConsumeCommand(cmd);
        }
    }

    void StoreCommandID(Command command)
    {
        instance.State.OwnedCommands.Add(command.RuntimeId);
    }

    void StoreAllCommandIDs(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var command))
                StoreCommandID(command);
        }
    }

    void StoreInputIntentSnapshot(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var command))
                instance.State.Intent = command.Intent;
        }
    }

    void LockCommand(Command command)
    {
        owner.Bus.Emit.Local(new CommandEvent(Request.Lock, command));
    }

    void LockAllCommands(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var cmd))
                LockCommand(cmd);
        }
    }

    void UnlockCommand(Command command)
    {
        owner.Bus.Emit.Local(new CommandEvent(Request.Unlock, command));
    }

    public void UnlockActiveWeaponCommands()
    {
        foreach (var action in instance.Action.Trigger)
        {
            if (activeBuffer.TryGetValue(action, out var cmd))
                UnlockCommand(cmd);
        }

    }

    // void UnlockAllCommands(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    // {
    //     foreach (var action in actions)
    //     {
    //         if (commands.TryGetValue(action, out var cmd))
    //             UnlockCommand(cmd);
    //     }
    // }

    // ============================================================================
    // Control System
    // ============================================================================

    public void UpdateAvailableControls()
    {

        switch (instance.State.Phase)
        {
            case WeaponPhase.Charging:
                AddControls(instance.Action.AddControlOnCharge);
                RemoveControls(instance.Action.RemoveControlOnCharge);
                break;

            case WeaponPhase.Fire:
                AddControls(instance.Action.AddControlOnFire);
                RemoveControls(instance.Action.RemoveControlOnFire);
                break;

            case WeaponPhase.FireEnd:
                AddControls(instance.Action.AddControlOnFireEnd);
                RemoveControls(instance.Action.RemoveControlOnFireEnd);

                if (instance.Action.SwapOnFire?.Length > 0)
                    instance.State.AvailableControls.Add(instance.Action.SwapOnFire);
                break;
        }

        if (instance.State.AvailableControls.Count > 0)
            Log.Trace("Control.Available", () => $"{string.Join(", ", instance.State.AvailableControls)}");
    }

    void AddControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
            instance.State.AvailableControls.Add(control);
    }

    void RemoveControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
            instance.State.AvailableControls.Remove(control);
    }


    // ===============================================================================
    //  Effect Management
    // ===============================================================================

    public void RequestEffects()
    {
        foreach (var effect in instance.Action.Effects)
        {
            if (ShouldApplyEffect(effect))
            {
                var API = new EffectAPI(instance, effect)
                {
                    Request = Request.Create
                };

                owner.Bus.Emit.Local(API.Request, API);
            }
        }
    }

    public void CancelEffects(WeaponInstance weaponInstance)
    {
        foreach (var instance in weaponInstance.State.OwnedEffects.Instances)
        {
            if (instance.Effect is ICancelable effect && effect.Cancelable)
            {
                var API = new EffectAPI(instance)
                {
                    Request = Request.Cancel
                };

                owner.Bus.Emit.Local(API.Request, API);
            }
        }
    }

    public void RequestClearOnReleaseEffects()
    {
        foreach (var owned in instance.State.OwnedEffects.Instances)
        {
            if (owned.Effect is ICancelable effect && effect.Cancelable && owned.Effect is ICancelableOnRelease cancelable && cancelable.CancelOnRelease)
            {
                var API = new EffectAPI(owned)
                {
                    Request = Request.Cancel
                };

                owner.Bus.Emit.Local(API.Request, API);
            }
        }
    }

    bool ShouldApplyEffect(Effect effect)
    {
        if (effect is ITrigger trigger) 
            return trigger.Trigger == instance.State.Phase;

        return instance.State.Phase == WeaponPhase.Enable;
    }


    // ============================================================================
    //  Aiming
    // ============================================================================

    void ResolveWeaponAimActivation()
    {
        if (owner is IAimable)
            WeaponAimProcessor.InitialiseAim(instance, Owner);
    }

    // ============================================================================
    //  Direction Management
    // ============================================================================

    public void RequestFacingDirection()
    {
        if (!(instance.Action.Direction.Enabled && instance.Action.Direction.SetTrigger == instance.State.Phase))
            return;

        var intent = instance.Action.Aim.Enabled ? instance.State.LiveIntent : instance.State.Intent;

        instance.State.LastFacingDirection = instance.Action.Direction.Source switch
        {
            DirectionSource.Aim             => instance.Action.Aim.Enabled ? Orientation.DirectionFromAngle(instance.State.CurrentAimAngle): intent.Aim,
            DirectionSource.Facing          => intent.Facing,
            DirectionSource.Direction       => intent.Direction,
            DirectionSource.LastDirection   => intent.LastDirection,
            _                               => intent.Direction,
        };

        owner.Bus.Emit.Local(new ForcedFacingEvent(Request.Set, instance.State.LastFacingDirection));
    }

    public void RequestClearDirection()
    {
        if (!(instance.Action.Direction.Enabled && instance.Action.Direction.ClearTrigger == instance.State.Phase))
            return;

        owner.Bus.Emit.Local(new ForcedFacingEvent(Request.Clear, Vector2.zero));
    }


    // ============================================================================
    //  Movement Management
    // ============================================================================

    public void RequestMovement()
    {
        foreach (var definition in instance.Action.MovementDefinitions)
        {
            if (definition.Phase  != instance.State.Phase)
                continue;

            definition.InputIntent = instance.State.Intent;

            owner.Bus.Emit.Local(new MovementEvent(owner, definition));
        }
    }

    public void ClearMovementFromPhase(WeaponPhase scopeToClear)
    {
        owner.Bus.Emit.Local(new ClearMovementScopeEvent(Request.Clear, owner, (int)scopeToClear));
    }
    
    public void RequestClearMovementFromOwner()
    {
        owner.Bus.Emit.Local(new ClearMovementScopeEvent(Request.Clear, owner, -1));
    }

    // ============================================================================
    //  Hitbox Management
    // ============================================================================

    public void RequestHitboxes()
    {
        foreach (var hitboxDefinition in instance.Action.Hitboxes)
        {            
            if (instance.State.Phase == hitboxDefinition.Lifetime.Phase)
            {
                hitboxDefinition.Direction.Input = instance.State.Intent;

                var packages = new List<object>
                {
                    CreateDamagePackage(),
                    CreateForcePackage()
                };

                var API = new HitboxAPI(owner, hitboxDefinition, packages)
                {
                    Request = Request.Create
                };

                hitboxRequestHandler.Forward(API.Id, API.Request, API);
            }
        }
    }

    public void RequestClearHitboxes()
    {
        foreach (var API in instance.State.OwnedHitboxes)
        {
            API.Request = Request.Destroy; 

            Emit.Global(API.Request, API);
        }
    }

    void HandleHitboxAPI(Message<Response, HitboxAPI> message)
    {
        var request = message.Payload;

        if (request.Response != Response.Success)
            return;

        if (request.owner != owner)
            return;

        instance?.State.OwnedHitboxes.Add(request);
    }

    void UpdateHitboxAngle()
    {
        foreach (var hitbox in instance.State.OwnedHitboxes)
        {
            if (hitbox.definition.Behavior.TrackAim)
            {
                Emit.Global(new HitboxDirectionUpdate
                {
                    HitboxId = hitbox.hitboxId,
                    AimAngle = instance.State.CurrentAimAngle
                });
            }
        }
    }

    // ============================================================================
    // Animation System
    // ============================================================================

        // Rework required blanket call to animation API?

    public void RequestAnimation()
    {
        string animation = instance.State.Phase switch
        {
            WeaponPhase.Charging    => instance.Action.Animations.OnCharge,
            WeaponPhase.Fire        => instance.Action.Animations.OnFire,
            WeaponPhase.FireEnd     => instance.Action.Animations.OnFireEnd,
            _ => null
        };

        if (animation is null)
            return;

        var API = new AnimationAPI(animation)
        {
            Request = Request.Play,
        };


        instance.State.AnimationAPI = API;

        if (instance.Action.HoldAnimationUntilReleased)
        {
            API.Settings.HoldUntilReleased = true;
        }

        if (instance.Action.LockAimDuringPlayback)
        {
            API.Data.Overrides.AddRange(AnimatorParameter.InputIntentSnapshot[typeof(IAimable)](instance.State.Intent));
        }

        if (instance.Action.LockDirectionDuringPlayback)
        {
            API.Data.Overrides.AddRange(AnimatorParameter.InputIntentSnapshot[typeof(IMovableActor)](instance.State.Intent));
        }
        

        owner.Bus.Emit.Local(API.Request, API);
        
    }


    public void RequestClearAnimation()
    {
        if (instance.State.AnimationAPI == null)
            return;
            
        if (!instance.Action.HoldAnimationUntilReleased)
            return;

        var API = instance.State.AnimationAPI;

        API.Request = Request.Stop;
        owner.Bus.Emit.Local(API.Request, API);
    }

    // ============================================================================
    //  Package
    // ============================================================================

    DamagePackage CreateDamagePackage()
    {
        var components = instance.Action.DamageComponents.ToList();
        return new DamagePackage(components);
    }

    ForcePackage CreateForcePackage()
    {
        var components = instance.Action.ForceComponents.ToList();
        return new ForcePackage(components);
    }

    // ============================================================================
    //  Cooldown
    // ============================================================================

    public void RegisterCooldown()
    {
        if (instance.Action.Cooldown > 0)
            cooldown.RegisterWeapon(instance.Action);
    }

    // ============================================================================
    //  Events
    // ============================================================================

    void HandleCommandPipelineUpdates(CommandPipelinesEvent message)
    {
        activeBuffer    = message.Active;
        inputBuffer     = message.Buffer;
    }

    void HandleEffectNonCancelableLockCount(EffectEvent message)
    {
        var instance = message.Instance;
    
        if (!IsDefinitiveAttackDisable(instance.Effect))
            return;
    
        switch (instance.State)
        {
            case EffectInstance.EffectState.Active:
                NonCancelableAttackLocks++;
                break;
            case EffectInstance.EffectState.Canceled:
            case EffectInstance.EffectState.Completed:
                NonCancelableAttackLocks--;
                break;
        }
    }

    void HandleLocksUpdate(LockUpdateEvent message)
    {
        locks = message.Locks;
    }

    void HandleEquipmentChange(EquipmentChangeEvent message)
    {
        if (message.Equipment is not Weapon weapon)
            return;

        switch(message.Type)
        {
            case Publish.Equipped:
                foreach (var action in weapon.Definition.Actions)
                    loadout.AddAction(action.Value, owner);
                break;
                
            case Publish.Unequipped:
                foreach (var action in weapon.Definition.Actions)
                    loadout.RemoveAction(action.Key);
                break;
        }
    }
    
        // ===================================
        //  Emitters
        // ===================================

    void SendWeaponEquip(WeaponAction Action)
    {
        owner.Bus.Emit.Local(new WeaponInstanceEvent(Publish.Equipped, agent, instance));
    }

    void SendWeaponUnequip()
    {
        owner.Bus.Emit.Local(new WeaponInstanceEvent(Publish.Unequipped, agent, instance));
    }

    void PublishTransition()
    {
        owner.Bus.Emit.Local(new WeaponInstanceEvent(Publish.Transitioned, agent, instance));
    }


    // ============================================================================
    //  Predicates
    // ============================================================================

    public bool HasInputBuffer()
    {
        return inputBuffer != null;
    }

    public bool HasActiveWeapon()
    {
        return instance != null;
    }

        // ===================================
        //  Activation Predicates
        // ===================================
    private bool HasPendingWeapon()
    {
        return pendingWeapon != null;
    }

    bool CanActivateFromDefaultControls(WeaponAction weapon)
    {
        if (weapon == null)
            return false;

        if (weapon.Name == instance?.Action.Name)
            return false;

        if (!validator.CanActivate(weapon))
            return false;

        if (!CanSetNewPending(weapon))
            return false;

        return true;
    } 

    bool CanActivateFromAvailableControls(WeaponAction weapon)
    {
        if (!HasAllRequiredTriggers(weapon))
            return false;

        if (!validator.CanActivate(weapon))
            return false;

        if (weapon.Availability == WeaponAvailability.OnHeld)
            return true;

        if (!HasNewCommandForWeapon(weapon))
            return false;

        if (!CanSetNewPending(weapon))
            return false;

        return true;
    }

    bool CanActivateFromInterruptControls(WeaponAction weapon)
    {
        if (weapon == null)
            return false;

        if (weapon.Type != WeaponType.Interrupt)
            return false;

        if (weapon.Name == instance.Action.Name)
            return false;

        if (!validator.CanInterrupt(weapon))
            return false;

        if (!validator.CanActivate(weapon, skipContextCheck: true))
            return false;
        
        if (!CanSetNewPending(weapon))
            return false;

        return true;
    }

    bool CanSetNewPending(WeaponAction weapon)
    {
        return pendingWeapon == null || weapon.Type >= pendingWeapon.Type;
    }
        // ===================================
        //  Release Predicates
        // ===================================

    bool ShouldReleaseWeapon()
    {
        return instance.State.ReadyToRelease;
    }

    bool ShouldDisableWeapon()
    {
        if (instance.Action.Activation == WeaponActivation.WhileHeld && instance.State.Phase == WeaponPhase.Fire)
            return false;

        if (instance.ShouldValidateActivationTriggers() && !HasAllRequiredTriggers(instance.Action))
            return true;

        if (ShouldTerminate())
            return true;

        return false;
    }

    bool ShouldTerminate()
    {
        return instance.Action.Termination switch
        {
            WeaponTermination.OnRelease     => instance.Action.Trigger.Any(trigger => !IsTriggerActive(trigger)),
            WeaponTermination.OnRootRelease => instance.Action.RequiredHeldTriggers.Any(trigger => !IsTriggerActive(trigger)),
            _ => false,
        };
    }

    // ============================================================================
    //  Helpers
    // ============================================================================

    public void StoreAndReleaseInstance()
    {
        StoreInstance();
        ClearInstance();
    }

    void CreateInstance(WeaponAction action)
    {
        instance = new WeaponInstance(owner, action);
    }
    
    void StoreInstance()
    {
        instance.State.Store();
        instanceStorage = instance;
    }

    void ClearInstance()
    {
        instance = null;
    }

    void ClearPendingWeapon()
    {
        pendingWeapon = null;
    }

    // ============================================================================
    //  Queries
    // ============================================================================

    private bool IsDefinitiveAttackDisable(Effect effect)
    {
        if (effect is not IDisableAttack disable)
            return false;

        if (!disable.DisableAttack)
            return false;

        if (effect is ICancelable cancelable && cancelable.Cancelable)
            return false;

        return true;
    }

    public IActivationStrategy GetActivationStrategy()
    {
        return activationStrategies[instance.Action.Activation];
    }



    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Weapons);

    void DebugLog()
    {
        Log.Debug("Weapon.Active",          () => instance?.Action.Name ?? "none");
        Log.Debug("Weapon.Phase",           () => instance?.State.Phase.ToString() ?? "none");
        Log.Debug("Weapon.Pending",         () => pendingWeapon?.Name);
        Log.Trace("Commands.Active",        () => activeBuffer?.Count > 0 ? string.Join(", ", activeBuffer?.Keys) : "");
        Log.Trace("Commands.Buffered",      () => inputBuffer?.Count > 0 ? string.Join(", ", inputBuffer?.Keys) : "");
        Log.Trace("Locks.Active",           () => locks == null ? "<none>" : string.Join(", ", locks.Select(kvp => $"{kvp.Key}({kvp.Value.Count})")) ); 
        Log.Trace("Locks.NonCancelable",    () => NonCancelableAttackLocks );
        Log.Trace("Cooldown",               () => cooldown.IsOnCooldown(instance?.Action.Name) ?  $"Remaining: {cooldown.GetRemainingCooldown(instance?.Action.Name)}" : "Ready");
    
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

    public override void Dispose()
    {
        hitboxRequestHandler.Dispose();
        Services.Lane.Deregister(this);
    }

    public Actor Owner                                                  => owner;
    public Agent Agent                                                  => agent;
    public WeaponInstance Instance                                      => instance;
    public WeaponCooldown Cooldown                                      => cooldown;
    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> Locks => locks;

    public UpdatePriority Priority => ServiceUpdatePriority.WeaponLogic;

    public WeaponInstance PreviousInstance => instanceStorage;
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct WeaponInstanceEvent : IMessage
{
    public readonly Agent Owner             { get; init; }
    public readonly WeaponInstance Instance { get; init; }
    public readonly Publish Type            { get; init; }

    public WeaponInstanceEvent(Publish type, Agent owner, WeaponInstance instance)
    {
        Owner       = owner;
        Instance    = instance;
        Type        = type;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Handlers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IWeaponPhaseHandler : IStateHandler<WeaponSystem>
{
    WeaponPhase Phase { get; }
}

public class EnablePhaseHandler : IWeaponPhaseHandler
{
    public void Enter(WeaponSystem controller)
    {
        controller.Instance.State.PhaseFrames.Start();

        if (controller.PreviousInstance != null)
            controller.CancelEffects(controller.PreviousInstance);

        controller.RequestEffects();

        controller.TransitionTo(WeaponPhase.Charging);
    }

    public void Update  (WeaponSystem controller)   {}

    public void Exit    (WeaponSystem controller)   
    {
        controller.ClearMovementFromPhase(Phase);
    }

    public WeaponPhase Phase => WeaponPhase.Enable;
}

public class ChargingPhaseHandler : IWeaponPhaseHandler
{
    public void Enter(WeaponSystem controller)
    {        
        controller.Instance.State.PhaseFrames.Restart();

        controller.UpdateAvailableControls  ();
        controller.RequestEffects           ();
        controller.RequestFacingDirection   ();
        controller.RequestMovement          ();
        controller.RequestHitboxes          ();
        controller.RequestAnimation         ();
    }

    public void Update(WeaponSystem controller)
    {
        if (ShouldFireOnChargeComplete(controller))
            controller.TransitionTo(WeaponPhase.Fire);
    }
    
    public void Exit(WeaponSystem controller)
    {
        controller.ClearMovementFromPhase(Phase);
    }

    bool ShouldFireOnChargeComplete(WeaponSystem controller)
    {
        return controller.GetActivationStrategy().ShouldFireOnChargeComplete(controller.Instance);
    }


    public WeaponPhase Phase => WeaponPhase.Charging;
}

public class FirePhaseHandler : IWeaponPhaseHandler
{
    public void Enter(WeaponSystem controller)
    {        
        controller.Instance.State.PhaseFrames.Restart();
        controller.Instance.State.HasFired = true;

        controller.UpdateAvailableControls  ();
        controller.RequestEffects           ();
        controller.RequestFacingDirection   ();
        controller.RequestMovement          ();
        controller.RequestHitboxes          ();
        controller.RequestAnimation         ();
    }

    public void Update(WeaponSystem controller)
    {
        if (HasHeldWeapon(controller.Instance))
            return;

        if (FireComplete(controller.Instance))
            controller.TransitionTo(WeaponPhase.FireEnd);
    }
    
    public void Exit(WeaponSystem controller)
    {
        controller.ClearMovementFromPhase(Phase);
    }

    // ===============================================================================

    bool HasHeldWeapon(WeaponInstance instance)
    {
        return instance.Action.Activation == WeaponActivation.WhileHeld;
    }

    bool FireComplete(WeaponInstance instance)
    {
        return instance.IsFireComplete();
    }

    public WeaponPhase Phase => WeaponPhase.Fire;
}

public class FireEndPhaseHandler : IWeaponPhaseHandler
{
    public void Enter(WeaponSystem controller)
    {        
        controller.Instance.State.PhaseFrames.Reset();

        ProcessControlWindow(controller);

        controller.UpdateAvailableControls  ();
        controller.RequestEffects           ();
        controller.RequestFacingDirection   ();
        controller.RequestMovement          ();
        controller.RequestHitboxes          ();
        controller.RequestAnimation         ();
    }

    public void Update(WeaponSystem controller)
    {
        if (!ControlWindowComplete(controller.Instance))
            return;
            
        controller.TransitionTo(WeaponPhase.Disable);
    }

    public void Exit(WeaponSystem controller)
    {
        controller.ClearMovementFromPhase(Phase);
    }

    // ===============================================================================

    void ProcessControlWindow(WeaponSystem controller)
    {
        if (controller.Instance.Action.ControlWindow > 0)
        {
            controller.Instance.State.ControlWindow = new ClockTimer(controller.Instance.Action.ControlWindow);
            controller.Instance.State.ControlWindow.Start();
        }
    }

    bool ControlWindowComplete(WeaponInstance instance)
    {
        if (instance.State.ControlWindow != null)
            return instance.State.ControlWindow.IsFinished;

        return true;
    }
    
    public WeaponPhase Phase => WeaponPhase.FireEnd;
}

public class DisablePhaseHandler : IWeaponPhaseHandler
{
    public void Enter(WeaponSystem controller)
    {
        controller.Instance.State.PhaseFrames.Reset();
        
        controller.RequestClearHitboxes();
        controller.RequestClearDirection();
        controller.RequestClearAnimation();
        controller.RequestClearOnReleaseEffects();
        controller.RequestClearMovementFromOwner();

        controller.UnlockActiveWeaponCommands();
        controller.RegisterCooldown();

        controller.Instance.State.ReadyToRelease = true;
    }

    public void Update  (WeaponSystem controller)   {}
    public void Exit    (WeaponSystem controller)   {}

    public WeaponPhase Phase => WeaponPhase.Disable;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Validation
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IActivationStrategy
{
    bool ShouldFireOnChargeComplete             (WeaponInstance weapon);
    bool ShouldTransitionOnFireRelease          (WeaponInstance weapon, WeaponSystem controller);
    bool ShouldTransitionOnChargeRelease        (WeaponInstance weapon, WeaponSystem controller);
}

public class OnPressActivationStrategy : IActivationStrategy
{
    public bool ShouldFireOnChargeComplete      (WeaponInstance weapon) => weapon.IsChargeComplete();
    public bool ShouldTransitionOnFireRelease   (WeaponInstance weapon, WeaponSystem controller) => false;
    public bool ShouldTransitionOnChargeRelease (WeaponInstance weapon, WeaponSystem controller) => false;
}

public class OnChargeActivationStrategy : IActivationStrategy
{
    public bool ShouldFireOnChargeComplete      (WeaponInstance weapon) => weapon.IsChargeComplete();
    public bool ShouldTransitionOnFireRelease   (WeaponInstance weapon, WeaponSystem controller) => false;
    public bool ShouldTransitionOnChargeRelease (WeaponInstance weapon, WeaponSystem controller) => false;
}

public class OnReleaseActivationStrategy : IActivationStrategy
{
    public bool ShouldFireOnChargeComplete      (WeaponInstance weapon) => weapon.IsChargeComplete() && weapon.Action.ForceMaxChargeRelease;
    public bool ShouldTransitionOnFireRelease   (WeaponInstance weapon, WeaponSystem controller) => false;
    public bool ShouldTransitionOnChargeRelease (WeaponInstance weapon, WeaponSystem controller)
    {
        bool inputReleased = weapon.Action.Trigger.Any(trigger => !controller.IsTriggerActive(trigger));
        bool chargedEnough = weapon.GetChargePercent() >= weapon.Action.MinimumChargeToFire;

        if (!inputReleased || !chargedEnough) return false;

        controller.TransitionTo(WeaponPhase.Fire);
        return true;
    }
}

public class WhileHeldActivationStrategy : IActivationStrategy
{
    public bool ShouldFireOnChargeComplete      (WeaponInstance weapon) => weapon.IsChargeComplete();
    public bool ShouldTransitionOnFireRelease   (WeaponInstance weapon, WeaponSystem controller)
    {
        bool inputReleased = weapon.Action.Trigger.Any(trigger => !controller.IsTriggerActive(trigger));

        if (!inputReleased) return false;

        controller.TransitionTo(WeaponPhase.FireEnd);
        return true;
    }
    public bool ShouldTransitionOnChargeRelease (WeaponInstance weapon, WeaponSystem controller) => false;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     Cooldown System
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class WeaponCooldownInstance
{
    public string name;
    public WeaponAction weapon;
    public DualCountdown timer;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public WeaponCooldownInstance(WeaponAction instance)
    {
        name    = instance.Name;
        weapon  = instance;
        timer   = new(instance.Cooldown);
    }

    public void Initialize()
    {
        timer.OnTimerStart += OnApply;
        timer.OnTimerStop  += OnClear;
        timer.Start();
    }

    public void Cancel()
    {
        timer.Cancel();
        OnCancel?.Invoke();
    }
}

public class WeaponCooldown
{
    readonly List<WeaponCooldownInstance> cooldowns = new();

    public void RegisterWeapon(WeaponAction weapon)
    {
        var instance = new WeaponCooldownInstance(weapon);

        instance.OnClear  += () => cooldowns.Remove(instance);
        instance.OnCancel += () => cooldowns.Remove(instance);

        cooldowns.Add(instance);
        instance.Initialize();
    }

    public bool IsOnCooldown(string weaponName)
    {
         return cooldowns.Any(instance => instance.name == weaponName);
    }

    public float GetRemainingCooldown(string weaponName)
    {
        return cooldowns.FirstOrDefault(instance => instance.name == weaponName)?.timer.CurrentTime ?? 0f;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Utilities
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class WeaponActivationValidator
{
    readonly WeaponSystem controller;

    public WeaponActivationValidator(WeaponSystem controller)
    {
        this.controller = controller;
    }

    // ===============================================================================

    public bool CanActivate(WeaponAction weapon, bool skipContextCheck = false)
    {
        var result = ValidateActivation(weapon, skipContextCheck);

        if (result.Success())
            Log.Trace("Validator.Failed", () => $"{weapon.Name} - {result.Reason}");

        return result.Success();
    }

    public WeaponValidationResult ValidateActivation(WeaponAction weapon, bool skipContextCheck = false)
    {
        WeaponValidationResult result;

        if (!(result = CheckCooldown(weapon)).Success())              return result;
        if (!(result = CheckActivationCondition(weapon)).Success())   return result;
        if (!(result = CheckActionLocks(weapon)).Success())           return result;
        if (!(result = CheckNonCancelableLocks(weapon)).Success())    return result;
        if (!skipContextCheck &&
            !(result = CheckContext(weapon)).Success())               return result;

        return WeaponValidationResult.Pass();
    }

    WeaponValidationResult CheckCooldown(WeaponAction weapon)
    {
        if (controller.Cooldown.IsOnCooldown(weapon.Name))
            return WeaponValidationResult.Fail($"Cooldown {controller.Cooldown.GetRemainingCooldown(weapon.Name)}s remaining");

        return WeaponValidationResult.Pass();
    }

    WeaponValidationResult CheckActivationCondition(WeaponAction weapon)
    {
        if (weapon.Condition.Activate != null && !weapon.Condition.Activate(controller.Owner))
            return WeaponValidationResult.Fail("Activate condition returned false");

        return WeaponValidationResult.Pass();
    }

    WeaponValidationResult CheckActionLocks(WeaponAction weapon)
    {
        if (!weapon.AcceptTriggerLockRequests)
            return WeaponValidationResult.Pass();

        if (weapon.CanCancelDisables)
            return WeaponValidationResult.Pass();

        foreach (var trigger in weapon.Trigger)
        {
            if (controller.Locks != null && controller.Locks.TryGetValue(trigger, out var lockList) && lockList.Count > 0)
                return WeaponValidationResult.Fail($"Trigger {trigger} has {lockList.Count} lock(s)");
        }

        return WeaponValidationResult.Pass();
    }

    WeaponValidationResult CheckNonCancelableLocks(WeaponAction incomingWeapon)
    {
        if (controller.HasActiveWeapon() && !controller.OnlyCancelableLocksRemain())
            return WeaponValidationResult.Fail($"{controller.NonCancelableAttackLocks} non-cancelable lock(s) active");

        return WeaponValidationResult.Pass();
    }


    WeaponValidationResult CheckContext(WeaponAction weapon)
    {
        if (!weapon.CanCancelDisables &&  controller.Owner is IAttacker agent && !agent.CanAttack)
            return WeaponValidationResult.Fail($"Context disallows attack (CanCancelDisables={weapon.CanCancelDisables})");

        return WeaponValidationResult.Pass();
    }

    // ===============================================================================

    public bool CanInterrupt(WeaponAction incomingWeapon)
    {
        var result = ValidateInterrupt(incomingWeapon);

        if (!result.Success())
            Log.Trace("Weapon.Interrupt", () => result.Reason);

        return result.Success();
    }

    public WeaponValidationResult ValidateInterrupt(WeaponAction incomingWeapon)
    {
        if (!controller.HasActiveWeapon())
            return WeaponValidationResult.Pass();

        if (!controller.OnlyCancelableLocksRemain() && !incomingWeapon.CanCancelDisables)
            return WeaponValidationResult.Fail("Non-cancelable locks remain and weapon cannot cancel disables");

        bool canCancelViaCondition  = controller.Instance.Action.Condition.Cancel != null && controller.Instance.Action.Condition.Cancel(controller.Owner);
        bool canCancelViaDisable    = incomingWeapon.CanCancelDisables;

        if (canCancelViaCondition || canCancelViaDisable)
            return WeaponValidationResult.Pass();

        return WeaponValidationResult.Fail("No valid cancel path");
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Weapons);
}


public readonly struct WeaponValidationResult
{
    public readonly Response    Response;
    public readonly string      Reason;

    private WeaponValidationResult(Response response, string reason)
    {
        Response    = response;
        Reason      = reason;
    }

    public static WeaponValidationResult Pass()               => new(Response.Success, "");
    public static WeaponValidationResult Fail(string reason)  => new(Response.Failure, reason);

    public bool Success() => Response == Response.Success;
    public bool Failure() => Response == Response.Failure;
}


