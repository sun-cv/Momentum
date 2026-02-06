

public class Shield : Weapon
{
    public Shield()
    {
        SlotType                    = EquipmentSlotType.OffHand;
        Definition                  = new ShieldDefinition(); 
    }
}




public class ShieldDefinition : WeaponDefinition
{
    public ShieldDefinition()
    {
        Name                        = "Shield";
        actions = new()
        {
            { "ShieldParry",        new ShieldParry()   },
            { "ShieldBlock",        new ShieldBlock()   },
            { "ShieldCharge",       new ShieldCharge()  },
            { "ShieldAim",          new ShieldAim()     },
            { "ShieldFire",         new ShieldFire()    },
            { "ShieldBash",         new ShieldBash()    }, 
        };
    }
}


public class ShieldParry : DamagingWeapon
{
    public ShieldParry()
    {
        Name                        = "ShieldParry";
        DefaultWeapon               = Capability.Attack2;
        Trigger                     = new() { Capability.Attack2 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        RequiredHeldTriggers        = new() { Capability.Attack2 };
        ChargeTimeFrames            = 1;
        FireDurationFrames          = 10;
        AddControlOnFireEnd         = new() { "ShieldBlock" };
        CanInterrupt                = true;
        CanCancelDisables           = true;
        Cooldown                    = .2f;
        Effects = new()
        {
            new ShieldParryWindow()
            {   
                Name                = "ShieldParryWindow",
                DurationFrames      = 60,
            },

            new ShieldMobility()
            {
                Name                = "ShieldParrySlow",
                Type                = EffectType.Speed,
                Trigger             = WeaponPhase.Fire,
                DurationFrames      = 5,
                Modifier            = .50f,
            },
        }; 
        Hitboxes = new()
        {
            new()
            {
                Prefab              = "HB_ShieldParry",
                Offset              = new(){ x = 0, y = 0 },
                FrameStart          = 1,
                FrameEnd            = 10,
                Lifetime            = HitboxLifetime.FrameBased,
                Behavior            = HitboxBehavior.Attached,
                Phase               = WeaponPhase.Fire,
                PersistPastSource   = true,
                AllowMultiHit       = false,
            }
        };
    }
}

public class ShieldBlock : DamagingWeapon
{
    public ShieldBlock()
    {
        Name                        = "ShieldBlock";
        Trigger                     = new() { Capability.Attack2 };
        Activation                  = WeaponActivation.WhileHeld;
        Termination                 = WeaponTermination.OnRelease;
        Availability                = WeaponAvailability.OnHeld;
        RequiredHeldTriggers        = new() { Capability.Attack2 };
        AcceptTriggerLockRequests   = false;
        FireDuration                = 9999;
        AddControlOnFire            = new() 
        { 
            "ShieldAim",
            "ShieldCharge",
            "ShieldBash"
        };

        Effects = new()
        {
            new ShieldBlockWindow()
            {
                Name                = "ShieldBlockWindow",
                Duration            = 9999,
                Cancelable          = true,
                CancelOnRelease     = true,

            },

            new ShieldBraceDisable()
            {   
                Name                = "ShieldBlockDisable",
                Trigger             = WeaponPhase.Fire,
                Cancelable          = false,
                DurationFrames      = 20,
                DisableAttack       = true,
                DisableRotate       = true,
            },

            new ShieldBraceDisable()
            {   
                Name                = "ShieldBlockDisableHeld",
                Trigger             = WeaponPhase.Fire,
                Cancelable          = true,
                CancelOnRelease     = true,
                Duration            = 9999,
                DisableAttack       = true,
                DisableRotate       = true,
            },

            new ShieldMobility()
            {
                Name                = "ShieldBlockSlow",
                Type                = EffectType.Grip,
                Trigger             = WeaponPhase.Fire,
                Cancelable          = true,
                CancelOnRelease     = true,
                DurationFrames      = 9999,
                Modifier            = .50f,
                ModifyTarget        = .25f,
                ModifyTimespan      = 2,
            }
        };
        Hitboxes = new()
        {
            new()
            {
                Prefab              = "HB_ShieldBlock",
                Offset              = new(){ x = 0, y = 0 },
                FrameStart          = 1,
                Behavior            = HitboxBehavior.Attached,
                Lifetime            = HitboxLifetime.Permanent,
                Phase               = WeaponPhase.Fire,
                AvailableDirections = HitboxDirection.Cardinal,
                AllowMultiHit       = false,
            }
        };
    }
}

public class ShieldAim : DamagingWeapon
{
    public ShieldAim()
    {
        Name                        = "ShieldAim";
        Trigger                     = new() { Capability.Attack2, Capability.Modifier };
        Activation                  = WeaponActivation.WhileHeld;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldTriggers        = new() { Capability.Attack2 };
        FireDuration                = 9999;
        AddControlOnFire            = new() { "ShieldFire" };
        AddControlOnFireEnd         = new() { "ShieldBlock" };
        CanCancelDisables           = true;

        Effects = new()
        {
            new ShieldBraceAim()
            {
                Trigger             = WeaponPhase.Fire,
                Name                = "ShieldAiming",
                Cancelable          = true,
                DurationFrames      = 9999,
            },
            new SwordSwingDisable()
            {   
                Name                = "ShieldAimDisable",
                Cancelable          = false,
                DurationFrames      = 5,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldAimDisableCancelable",
                Cancelable          = true,
                DurationFrames      = 9999,
                DisableRotate       = true,
            }
        };
    }
}

public class ShieldFire : DamagingWeapon
{
    public ShieldFire()
    {
        Name                        = "ShieldFire";
        Trigger                      = new() { Capability.Attack2, Capability.Modifier, Capability.Attack1 };
        Activation                  = WeaponActivation.OnRelease;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldTriggers        = new() { Capability.Attack2 };
        ChargeTimeFrames            = 20;
        FireDuration                = 25;
        ForceMaxChargeRelease       = true;
        MinimumChargeToFire         = 0.3f;
        AddControlOnFireEnd         = new() { "ShieldAim" };
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "ShieldFireDisable",
                Cancelable          = false,
                DurationFrames      = 18,
                DisableAttack       = true,
                DisableRotate       = true,
                DisableMove         = true,
                RequestActionLock   = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldFireDisableCancelable",
                Cancelable          = true,
                DurationFrames      = 30,
                DisableAttack       = true,
                RequestActionLock   = true,
            }
        };
    }
}

public class ShieldBash : DamagingWeapon
{
    public ShieldBash()
    {
        Name                        = "ShieldBash";
        Trigger                      = new() { Capability.Attack2, Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldTriggers        = new() { Capability.Attack2 };
        ChargeTimeFrames            = 5;
        FireDuration                = 20;
        AddControlOnFireEnd         = new() { "ShieldBlock" };
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "ShieldBashDisable",
                Cancelable          = false,
                DurationFrames      = 15,
                DisableAttack       = true,
                DisableRotate       = true,
                DisableMove         = true,
                RequestActionLock   = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldBashDisableCancelable",
                Cancelable          = true,
                DurationFrames      = 25,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
            }
        };
    }
}

public class ShieldCharge : DamagingWeapon
{
    public ShieldCharge()
    {

        
        Name                        = "ShieldCharge";
        Trigger                      = new() { Capability.Attack2, Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldTriggers        = new() { Capability.Attack2 };
        ChargeTimeFrames            = 5;
        FireDuration                = 20;
        ControlWindow               = 0.3f;
        SwapOnFire                  = "ShieldFire";
        AddControlOnFireEnd         = new() { "ShieldBlock" };
        Effects = new()
        {
            new ShieldMobility()
            {
                Trigger             = WeaponPhase.Fire,
                Name                = "ShieldChargeMovement",
                DurationFrames      = 30,
                Cancelable          = true,
            },
            new SwordSwingDisable()
            {   
                Name                = "ShieldChargeDisable",
                Cancelable          = false,
                DurationFrames      = 20,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldChargeDisableCancelable",
                Cancelable          = true,
                DurationFrames      = 35,
                DisableAttack       = true,
                RequestActionLock   = true,
            }
        };
    }
}
