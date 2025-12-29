public class SwordAndShield : WeaponSet
{
    public SwordAndShield()
    {
        weapons = new()
        {
            { "SwordStrike",        new SwordStrike()       },
            { "SwordCleave",        new SwordCleave()       },
            { "SwordRend",          new SwordRend()         },
            { "ShieldParry",        new ShieldParry()       },
            { "ShieldBlock",        new ShieldBlock()       },
            { "ShieldCharge",       new ShieldCharge()      },
            { "ShieldAim",          new ShieldAim()         },
            { "ShieldFire",         new ShieldFire()        },
            { "ShieldBash",         new ShieldBash()        }, 
            { "SwordAndShieldDash", new SwordAndShieldDash()},
        };
    }
}

// ============================================================================
// SWORD COMBO CHAIN
// ============================================================================

public class SwordStrike : DamagingWeapon
{
    public SwordStrike()
    {
        Name                        = "SwordStrike";
        DefaultWeapon               = Capability.Attack1;
        Action                      = new() { Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 3;
        FireDurationFrames          = 20;
        ControlWindow               = 0.3f;
        SwapOnFire                  = "SwordCleave";
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "SwordSwingDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 9,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 20,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            }
        };
    }
}

public class SwordCleave : DamagingWeapon
{
    public SwordCleave()
    {
        Name                        = "SwordCleave";
        Action                      = new() { Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.OnPhase;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 0;
        FireDurationFrames          = 15;
        ControlWindow               = 0.3f;
        SwapOnFire                  = "SwordRend";
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "SwordSwingDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 15,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 25,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            }
        };
    }
}

public class SwordRend : DamagingWeapon
{
    public SwordRend()
    {
        Name                        = "SwordRend";
        Action                       = new() { Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.OnPhase;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 0;
        FireDurationFrames          = 25;
        ControlWindow               = 0.3f;
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "SwordSwingDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 20,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 25,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            }
        };
    }
}

// ============================================================================
// SHIELD CHAIN
// ============================================================================

public class ShieldParry : DamagingWeapon
{
    public ShieldParry()
    {
        Name                        = "ShieldParry";
        DefaultWeapon               = Capability.Attack2;
        Action                       = new() { Capability.Attack2 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        RequiredHeldActions         = new() { Capability.Attack2 };
        ChargeTimeFrames            = 1;
        FireDurationFrames          = 40;
        AddControlOnFireEnd         = new() { "ShieldBlock" };
        Effects = new()
        {
            new ShieldParryActivation()
            {   
                Name                = "ShieldParryActivation",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 40,
            },

            new ShieldMobility()
            {
                Name                = "ShieldParrySlow",
                Type                = EffectType.Speed,
                Trigger             = WeaponPhase.Fire,
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 40,
                Modifier            = .50f,
            },
        }; 
 
    }
}

public class ShieldBlock : DamagingWeapon
{
    public ShieldBlock()
    {
        Name                        = "ShieldBlock";
        Action                       = new() { Capability.Attack2 };
        Activation                  = WeaponActivation.WhileHeld;
        Termination                 = WeaponTermination.OnRelease;
        Availability                = WeaponAvailability.OnHeld;
        RequiredHeldActions         = new() { Capability.Attack2 };
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
            new ShieldBlockActivation()
            {
                Name                = "ShieldBlockActivation",
                Duration            = 9999,
                Active              = true,
                Cancelable          = true,
            },

            new ShieldBraceDisable()
            {   
                Name                = "ShieldBlockDisable",
                Trigger             = WeaponPhase.Fire,
                Active              = true,
                Cancelable          = false,
                Duration            = .5f,
                DisableAttack       = true,
                DisableRotate       = true,
            },

            new ShieldBraceDisable()
            {   
                Name                = "ShieldBlockDisable",
                Trigger             = WeaponPhase.Fire,
                Active              = true,
                Cancelable          = true,
                Duration            = 9999,
                DisableAttack       = true,
                DisableRotate       = true,
            },

            new ShieldMobility()
            {
                Name                = "ShieldBlockSlow",
                Type                = EffectType.Grip,
                Trigger             = WeaponPhase.Fire,
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 9999,
                Modifier            = .50f,
                ModifierTarget      = .25f,
                ModifierSpeed       = 2,
            }
        };
    }
}

public class ShieldAim : DamagingWeapon
{
    public ShieldAim()
    {
        Name                        = "ShieldAim";
        Action                      = new() { Capability.Attack2, Capability.Modifier };
        Activation                  = WeaponActivation.WhileHeld;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldActions         = new() { Capability.Attack2 };
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
                Active              = true,
                DurationFrames      = 9999,
                Cancelable          = true,
            },
            new SwordSwingDisable()
            {   
                Name                = "ShieldAimDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 5,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldAimDisableCancelable",
                Active              = true,
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
        Action                       = new() { Capability.Attack2, Capability.Modifier, Capability.Attack1 };
        Activation                  = WeaponActivation.OnRelease;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldActions         = new() { Capability.Attack2 };
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
                Active              = true,
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
                Active              = true,
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
        Action                       = new() { Capability.Attack2, Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldActions         = new() { Capability.Attack2 };
        ChargeTimeFrames            = 5;
        FireDuration                = 20;
        AddControlOnFireEnd         = new() { "ShieldBlock" };
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "ShieldBashDisable",
                Active              = true,
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
                Active              = true,
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
        Action                       = new() { Capability.Attack2, Capability.Dash };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldActions         = new() { Capability.Attack2 };
        ChargeTimeFrames            = 8;
        FireDuration                = 30;
        ControlWindow               = 0.3f;
        SwapOnFire                  = "ShieldFire";
        AddControlOnFireEnd         = new() { "ShieldBlock" };
        Effects = new()
        {
            new ShieldMobility()
            {
                Trigger             = WeaponPhase.Fire,
                Name                = "ShieldChargeMovement",
                Active              = true,
                DurationFrames      = 30,
                Cancelable          = true,
            },
            new SwordSwingDisable()
            {   
                Name                = "ShieldChargeDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 20,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldChargeDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 35,
                DisableAttack       = true,
                RequestActionLock   = true,
            }
        };
    }
}

// ============================================================================
// DASH
// ============================================================================

public class SwordAndShieldDash : MovementWeapon
{
    public SwordAndShieldDash()
    {
        Name                        = "SwordAndShieldDash";
        DefaultWeapon               = Capability.Dash;
        Action                       = new() { Capability.Dash };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 2;
        FireDurationFrames          = 10;
        Cooldown                    = 0.2f;
        WeaponOverridesMovement     = true;
        Speed                       = 20;
        Modifier                    = 1;
        LockDirection               = true;
        CanCancelDisables           = true;
        CanInterrupt                = true;
        Effects = new()
        {
            new DashDisable()
            {   
                Name                = "DashDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 20,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Dash }
            },

            new DashMobility()
            {
                Trigger             = WeaponPhase.FireEnd,
                Name                = "DashMobilityDisable",
                Type                = EffectType.Speed,
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 15,
                Modifier            = .0f,
            },

            new DashMobility()
            {
                Trigger             = WeaponPhase.FireEnd,
                Name                = "DashMobilitySlow",
                Type                = EffectType.Grip,
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 30,
                Modifier            = .0f,
                ModifierTarget      = 1.0f,
                ModifierSpeed       = .5f,
            },

        };
    }
}