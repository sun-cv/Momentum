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
        DefaultWeapon               = InputIntent.Attack1;
        Input                       = new() { InputIntent.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 3;
        FireDurationFrames          = 15;
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
                RequestTriggerLock  = true,
                TriggerLocks        = new(){ InputIntent.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 15,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestTriggerLock  = true,
                TriggerLocks        = new(){ InputIntent.Attack1 }
            }
        };
    }
}

public class SwordCleave : DamagingWeapon
{
    public SwordCleave()
    {
        Name                        = "SwordCleave";
        Input                       = new() { InputIntent.Attack1 };
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
                RequestTriggerLock  = true,
                TriggerLocks        = new(){ InputIntent.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 25,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestTriggerLock  = true,
                TriggerLocks        = new(){ InputIntent.Attack1 }
            }
        };
    }
}

public class SwordRend : DamagingWeapon
{
    public SwordRend()
    {
        Name                        = "SwordRend";
        Input                       = new() { InputIntent.Attack1 };
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
                RequestTriggerLock  = true,
                TriggerLocks        = new(){ InputIntent.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 25,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestTriggerLock  = true,
                TriggerLocks        = new(){ InputIntent.Attack1 }
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
        DefaultWeapon               = InputIntent.Attack2;
        Input                       = new() { InputIntent.Attack2 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        RequiredHeldInputs          = new() { InputIntent.Attack2 };
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
                Type                = "SPEED",
                Trigger             = WeaponPhase.Fire,
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 10,
                Modifier            = .25f,
            },
            new ShieldMobility()
            {
                Name                = "ShieldParrySlowTransition",
                Type                = "SPEED",
                Trigger             = WeaponPhase.Fire,
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 60,
                Modifier            = .50f,
            }
        }; 
 
    }
}

public class ShieldBlock : DamagingWeapon
{
    public ShieldBlock()
    {
        Name                        = "ShieldBlock";
        Input                       = new() { InputIntent.Attack2 };
        Activation                  = WeaponActivation.WhileHeld;
        Termination                 = WeaponTermination.OnRelease;
        Availability                = WeaponAvailability.OnHeld;
        RequiredHeldInputs          = new() { InputIntent.Attack2 };
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
                Cancelable          = true,
                Duration            = 9999,
                DisableAttack       = true,
                DisableRotate       = true,
            },

            new ShieldMobility()
            {
                Name                = "ShieldBlockSlow",
                Type                = "SPEED",
                Trigger             = WeaponPhase.Fire,
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 9999,
                Modifier            = .50f,
            }
        };
    }
}

public class ShieldAim : DamagingWeapon
{
    public ShieldAim()
    {
        Name                        = "ShieldAim";
        Input                       = new() { InputIntent.Attack2, InputIntent.Modifier };
        Activation                  = WeaponActivation.WhileHeld;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldInputs          = new() { InputIntent.Attack2 };
        FireDuration                = 9999;
        AddControlOnFire            = new() { "ShieldFire" };
        AddControlOnFireEnd         = new() { "ShieldBlock" };

        Condition   = new()
        {
            Activate    = (commands) =>  true,
            Cancel      = (commands) =>  false,
        };

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
                RequestTriggerLock  = true,
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
        Input                       = new() { InputIntent.Attack2, InputIntent.Modifier, InputIntent.Attack1 };
        Activation                  = WeaponActivation.OnRelease;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldInputs          = new() { InputIntent.Attack2 };
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
                RequestTriggerLock  = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldFireDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 30,
                DisableAttack       = true,
                RequestTriggerLock  = true,
            }
        };
    }
}

public class ShieldBash : DamagingWeapon
{
    public ShieldBash()
    {
        Name                        = "ShieldBash";
        Input                       = new() { InputIntent.Attack2, InputIntent.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldInputs          = new() { InputIntent.Attack2 };
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
                RequestTriggerLock  = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldBashDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 25,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestTriggerLock  = true,
            }
        };
    }
}

public class ShieldCharge : DamagingWeapon
{
    public ShieldCharge()
    {
        Name                        = "ShieldCharge";
        Input                       = new() { InputIntent.Attack2, InputIntent.Dash };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.OnRootRelease;
        Availability                = WeaponAvailability.OnPhase;
        RequiredHeldInputs          = new() { InputIntent.Attack2 };
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
                RequestTriggerLock  = true,
            },
            new SwordSwingDisable()
            {
                Name                = "ShieldChargeDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 35,
                DisableAttack       = true,
                RequestTriggerLock  = true,
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
        DefaultWeapon               = InputIntent.Dash;
        Input                       = new() { InputIntent.Dash };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 2;
        FireDuration                = 15;
        CanCancelDisables           = true;
        Effects = new()
        {
            new ShieldMobility()
            {
                Trigger             = WeaponPhase.Fire,
                Name                = "DashMovement",
                Active              = true,
                DurationFrames      = 15,
                Cancelable          = false,
            },
            new SwordSwingDisable()
            {
                Name                = "DashDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 10,
                DisableAttack       = true,
                RequestTriggerLock  = true,
            },
            new SwordSwingDisable()
            {
                Name                = "DashDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 20,
                DisableAttack       = true,
                RequestTriggerLock  = true,
            }
        };
    }
}