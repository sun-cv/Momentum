// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// // ============================================================================
// // WEAPON MOVEMENT STATE
// // ============================================================================

// public readonly struct WeaponMovementState
// {
//     public readonly bool  CancelMovement;
//     public readonly bool  LockDirection;
//     public readonly bool  OverrideMovement;
//     public readonly float VelocityOverride;
//     public readonly float ModifierOverride;
//     public readonly DirectionSource DirectionSource;
    
//     public WeaponMovementState(WeaponAction action, WeaponPhase phase)
//     {
//         CancelMovement      = ResolveCancelMovement  (action, phase);
//         LockDirection       = ResolveLockDirection   (action, phase);
//         VelocityOverride    = ResolveVelocityOverride(action, phase);
//         ModifierOverride    = ResolveModifierOverride(action, phase);
//         OverrideMovement    = ResolveOverrideMovement(action, phase);
//         DirectionSource     = action.LockDirectionSource;
//     }
    
//     static bool ResolveCancelMovement(WeaponAction action, WeaponPhase phase)
//     {
//         if (action.CancelMovement)
//             return true;

//         return phase switch
//         {
//             WeaponPhase.Charging => action.ChargeCancelMovement,
//             _ => false
//         };
//     }
    
//     static bool ResolveLockDirection(WeaponAction action, WeaponPhase phase)
//     {
//         if (action.LockDirection)
//             return true;

//         return phase switch
//         {
//             WeaponPhase.Charging    => action.LockDirectionOnCharge,
//             _ => false
//         };
//     }
    
//     static float ResolveVelocityOverride(WeaponAction action, WeaponPhase phase)
//     {
//         if (action.Velocity >= 0)
//             return action.Velocity;

//         return phase switch
//         {
//             WeaponPhase.Charging when action.ChargeVelocity >= 0 => action.ChargeVelocity,
//             _ => -1
//         };
//     }
    
//     static float ResolveModifierOverride(WeaponAction action, WeaponPhase phase)
//     {
//         if (phase == WeaponPhase.Fire && action is MovementWeapon movement && movement.Modifier >= 0)
//             return movement.Modifier;
//         return -1;
//     }
    
//     static bool ResolveOverrideMovement(WeaponAction action, WeaponPhase phase)
//     {
//         if (action.WeaponOverridesMovement)
//             return true;

//         return phase switch
//         {
//             _ => false
//         };
//     }

//     public static WeaponMovementState None => default;
// }



// // ============================================================================
// // MOVEMENT ENGINE
// // ============================================================================

// public class MovementEngine : IServiceTick
// {
//     readonly float maxSpeed     = Settings.Movement.MAX_SPEED;
//     readonly float acceleration = Settings.Movement.ACCELERATION;
//     readonly float friction     = Settings.Movement.FRICTION;

//     readonly bool normalizeVelocity = false;

//     EffectCache     cache;

//     Actor           owner;
//     IMovableActor   actor;
//     Rigidbody2D     body;

//     WeaponInstance  instance;
//     WeaponState     state;
//     WeaponAction    weapon;
//     WeaponPhase     phase;
    
//     WeaponMovementState weaponMovementState;


//     float   speed;
//     float   modifier                                            = 1.0f;
//     Dictionary<EffectType, List<IMovementModifier>> modifiers   = new();
//     bool    lockDirection;

//     Vector2 direction;
//     Vector2 directionLock;
//     Vector2 directionTrail;

//     Vector2 momentum;
//     Vector2 velocity;

//     Vector2 movementStartPosition;

//     public MovementEngine(Actor actor)
//     {
//         if (actor.Bridge is not ActorBridge bridge)
//         {
//             Log.Error(LogSystem.Movement, LogCategory.Activation, () => $"Movement Engine activation requires Actor Bridge (actor {actor.RuntimeID} failed)");
//             return;
//         }

//         GameTick.Register(this);

//         cache = new((effectInstance) => effectInstance.Effect is IType instance && (instance.Type == EffectType.Speed || instance.Type == EffectType.Grip));

//         cache.OnApply   += CreateModifier;
//         cache.OnCancel  += ClearModifier;
//         cache.OnClear   += ClearModifier;
        
//         weaponMovementState = WeaponMovementState.None;

//         EventBus<WeaponPublish>.Subscribe(HandleWeaponPublish);

//         this.owner          = actor;
//         this.body           = bridge.Body;
//         this.actor          = actor as IMovableActor;

//         body.freezeRotation = true;
//         body.gravityScale   = 0;
//         body.interpolation  = RigidbodyInterpolation2D.Interpolate;
//     }

//     public void Tick()   
//     {
//         SetDirection();
//         SetSpeed();
                
//         CalculateModifier();
//         CalculateVelocity();

//         ApplyFriction();

//         if (CanMove())
//             ApplyVelocity();


//         DebugLog();
//     }


//     bool CanMove()
//     {
//         return owner is IMovable actor && actor.CanMove;
//     }

//     // ============================================================================
//     // MOVEMENT CALCULATIONS
//     // ============================================================================

//     void CalculateVelocity()
//     {
//         if (HasActiveWeapon())
//             ApplyWeaponInfluencedVelocity();
//         else
//             ApplyNormalVelocity();
//     }
    
//     void ApplyNormalVelocity()
//     {
//         Vector2 targetVelocity = Mathf.Clamp(speed * modifier, 0, maxSpeed) * direction;

//         velocity = Vector2.MoveTowards(velocity, targetVelocity, acceleration * Clock.DeltaTime);
//         momentum = velocity;
//     }
    
//     void ApplyWeaponInfluencedVelocity()
//     {
//         if (weaponMovementState.CancelMovement)
//         {
//             velocity = Vector2.zero;
//             momentum = velocity;
//             return;
//         }
        
//         Vector2 effectiveDirection = ResolveDirection();

//         float effectiveSpeed    = weaponMovementState.VelocityOverride != -1 ?  weaponMovementState.VelocityOverride : speed;
//         float effectiveModifier = weaponMovementState.ModifierOverride != -1 ?  weaponMovementState.ModifierOverride : modifier;
        
//         Vector2 targetVelocity  = Mathf.Clamp(effectiveSpeed * effectiveModifier, 0, maxSpeed) * effectiveDirection;
        
//         velocity = weaponMovementState.OverrideMovement ? targetVelocity : Vector2.MoveTowards(velocity, targetVelocity, acceleration * Clock.DeltaTime);
//         momentum = velocity;
//     }
    
//     Vector2 ResolveDirection()
//     {
//         if (weaponMovementState.LockDirection)
//         {
//             if (!lockDirection)
//             {
//                 directionLock = GetDirectionFromSource(weaponMovementState.DirectionSource);
//                 lockDirection = true;
//             }
//             return directionLock;
//         }

//         lockDirection = false;
//         return direction;
//     }


//     void CalculateModifier()
//     {
//         float sumOfAverages = 0f;
//         int typeCount       = 0;
    
//         foreach (var list in modifiers.Values)
//         {
//             int count = list.Count;
//             if (count == 0) continue;
    
//             float sum = 0f;

//             for (int i = 0; i < count; i++)
//                 sum += list[i].Resolve();
    
//             sumOfAverages += sum / count;
//             typeCount++;
//         }
    
//         modifier = typeCount == 0 ? 1f : sumOfAverages / typeCount;
//     }

//     void ApplyFriction() => velocity *= 1 - Mathf.Clamp01(friction * Clock.DeltaTime);
//     void ApplyVelocity() =>  body.MovePosition(body.position + velocity * Time.fixedDeltaTime);

//     // ============================================================================
//     // WEAPON MANAGEMENT
//     // ============================================================================



//     void HandleWeaponPublish(WeaponPublish evt)
//     {

//         switch(evt.Action)
//         {
//             case Publish.Equipped when evt.Payload.Instance.Action.Name == "SwordAndShieldDash":
//                 movementStartPosition = body.position;
//                 break;

//             case Publish.Released when weapon?.Name == "SwordAndShieldDash":
//                 float distance = Vector2.Distance(movementStartPosition, body.position);
//                 Log.Debug(LogSystem.Movement, LogCategory.State, () => $"Distance: {distance:F3}");
//                 break;
//         }

//         switch(evt.Action)
//         {
//             case Publish.Equipped:
//                 instance            = evt.Payload.Instance;
//                 state               = instance.State;
//                 weapon              = instance.Action;
//                 weaponMovementState = new WeaponMovementState(weapon, WeaponPhase.Idle);
//                 break;
                
//             case Publish.PhaseChange:
//                 phase               = state.Phase;
//                 weaponMovementState = new WeaponMovementState(weapon, phase);
//                 break;
                
//             case Publish.Released:
//                 weapon              = null;
//                 weaponMovementState = WeaponMovementState.None;
//                 lockDirection       = false;
//                 break;
//         }



//     }

//     // ============================================================================
//     // EFFECT MODIFIER MANAGEMENT
//     // ============================================================================

//     void CreateModifier(EffectInstance instance)
//     {
//         var coded = instance.Effect as IType;

//         if (!modifiers.ContainsKey(coded.Type))
//             modifiers[coded.Type] = new List<IMovementModifier>();

//         modifiers[coded.Type].Add(CreateModifierForType(coded.Type, instance));
//     }

//     void ClearModifier(EffectInstance instance)
//     {
//         var coded = instance.Effect as IType;

//         if (modifiers.TryGetValue(coded.Type, out var list))
//         {
//             var modifier = list.FirstOrDefault(modifier => modifier.Instance == instance);
//             if (modifier != null)
//                 list.Remove(modifier);
//         }
//     }

//     IMovementModifier CreateModifierForType(EffectType type, EffectInstance instance)
//     {
//         return type switch
//         {
//             EffectType.Speed => new SpeedMovementModifier(instance),
//             EffectType.Grip  => new GripMovementModifier(instance),
//             _ => null
//         };
//     }

//     // ============================================================================
//     // HELPER METHODS
//     // ============================================================================

//     Vector2 GetDirectionFromSource(DirectionSource source)
//     {
//         switch (source)
//         {
//             case DirectionSource.Movement:
//                 if (direction.sqrMagnitude > 0.01f)
//                     return direction.normalized;
//                 if (directionTrail.sqrMagnitude > 0.01f)
//                     return directionTrail.normalized;
//                 return Vector2.down;

//             case DirectionSource.Intent:
//                 if (actor is IHasAim intentActor)
//                 {
//                     Vector2 intentDir = intentActor.AimDirection;
//                     if (intentDir.sqrMagnitude > 0.01f)
//                         return intentDir.normalized;
//                 }
//                 goto case DirectionSource.Movement;

//             case DirectionSource.CardinalIntent:
//                 if (actor is IHasAim cardinalActor)
//                 {
//                     Vector2 intentDir = Orientation.ToVector(cardinalActor.CardinalAimDirection);
//                     if (intentDir.sqrMagnitude > 0.01f)
//                         return intentDir.normalized;
//                 }
//                 goto case DirectionSource.Movement;

//             case DirectionSource.Facing:
//                 if (actor is IOrientable orientable)
//                 {
//                     Vector2 facingDir = Orientation.ToVector(orientable.CardinalDirection);
//                     if (facingDir.sqrMagnitude > 0.01f)
//                         return facingDir.normalized;
//                 }
//                 goto case DirectionSource.Movement;
//             default:
//                 return Vector2.right;
//         }
//     }

//     bool HasActiveWeapon() => weapon != null;

//     void SetDirection()
//     {

//         if (actor.Direction == Vector2.zero && direction != Vector2.zero)
//             directionTrail = direction;

//         Vector2 inputDir = actor.Direction;

//         if (normalizeVelocity && inputDir.sqrMagnitude > 1f)
//             inputDir = inputDir.normalized;

//         direction = inputDir;
//     }


//     void SetSpeed()
//     {
//         speed = actor.Speed;
//     }

//     void DebugLog()
//     {
//         Log.Debug(LogSystem.Movement, LogCategory.Control,"Movement", "Movement.Speed",     () => speed);  
//         Log.Debug(LogSystem.Movement, LogCategory.State,  "Movement", "Movement.Velocity",  () => velocity);
//         Log.Debug(LogSystem.Movement, LogCategory.State,  "Movement", "Movement.Modifier",  () => modifier);
//         Log.Trace(LogSystem.Movement, LogCategory.Effect, "Movement", "Effect.Cache",       () => cache.Effects.Count);
//         Log.Debug(LogSystem.Movement, LogCategory.Effect, "Movement", "Effect.Active",      () => $"{string.Join(", ", cache.Effects.Select(effect => effect.Effect.Name))}");
//     }

//     public Vector2 Velocity => velocity;
//     public Vector2 Momentum => momentum;

//     public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
// }


