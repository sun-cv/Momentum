




public class Sword : Weapon
{
    public Sword()
    {
        SlotType                    = EquipmentSlotType.MainHand;
        Definition                  = new SwordDefinition(); 
    }
}


public class SwordDefinition : WeaponDefinition
{
    public SwordDefinition()
    {
        Name                        = "Sword";
        actions = new()
        {
            { "SwordStrike",        new SwordStrike()   },
            { "SwordCleave",        new SwordCleave()   },
            { "SwordRend",          new SwordRend()     },
        };
    }
}


public class SwordStrike : DamagingWeapon
{
    public SwordStrike()
    {
        Name                        = "SwordStrike";
        DefaultWeapon               = Capability.Attack1;
        Trigger                     = new() { Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 3;
        FireDurationFrames          = 30;
        ControlWindow               = 0.3f;
        AddControlOnFire            = new() { "SwordCleave"};
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "SwordSwingDisable",
                Cancelable          = false,
                DurationFrames      = 30,
                DisableAttack       = true,
                DisableRotate       = true,
                DisableMove         = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Cancelable          = true,
                DurationFrames      = 60,
                DisableAttack       = true,
                DisableRotate       = true,
                DisableMove         = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },

        };

        MovementDefinitions = new()
        {
            new()
            {
                KinematicAction     = KinematicAction.Lunge,
                Speed               = 5,
                SpeedCurve          = new(new(0f, 1f, 0f, -0.25f), new(1f, .25f, 0f, 0f)),
                DurationFrames      = 15,
                PersistPastScope    = true,
                Phase               = WeaponPhase.Fire,
                Scope               = (int)WeaponPhase.Fire,
            }
        };

        DamageComponents    = new()
        {
            new()
            {
                Amount              = 10,
                ForceMagnitude      = 1,
                Effects             = new(),
            }
        };

        Hitboxes = new()
        {
            new()
            {
                Form                = new()
                {
                    Prefab          = "HB_SwordSwing",
                    Offset          = new(){ x = 0, y = 0 },

                },
                Behavior            = new()
                {
                    Type            = HitboxBehavior.Attached,
                    AllowMultiHit   = false,
                },
                Direction           = new()
                {
                    Scope           = HitboxDirectionScope.Intercardinal
                },
                Lifetime            = new()
                {
                    FrameStart      = 1,
                    FrameEnd        = 20,
                    Phase           = WeaponPhase.Fire,
                },
            },
        };
        
        Animations = new()
        {
            OnFire = HeroAnimation.SwordStrike,
        };
    }
}

public class SwordCleave : DamagingWeapon
{
    public SwordCleave()
    {
        Name                        = "SwordCleave";
        Trigger                     = new() { Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.OnPhase;
        LockTriggerAction           = true;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 3;
        FireDurationFrames          = 30;
        ControlWindow               = 0.3f;
        CanCancelDisables           = true;
        AddControlOnFire            = new() { "SwordRend"};
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "SwordSwingDisable",
                Cancelable          = false,
                DurationFrames      = 30,
                DisableAttack       = true,
                DisableRotate       = true,
                DisableMove         = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Cancelable          = true,
                DurationFrames      = 60,
                DisableAttack       = true,
                DisableMove         = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            }
        };

        MovementDefinitions = new()
        {
            new()
            {
                KinematicAction     = KinematicAction.Lunge,
                Speed               = 5,
                SpeedCurve          = new(new(0f, 1f, 0f, -0.25f), new(1f, .25f, 0f, 0f)),
                DurationFrames      = 15,
                PersistPastScope    = true,
                Phase               = WeaponPhase.Fire,
                Scope               = (int)WeaponPhase.Fire,
            }
        };

        DamageComponents    = new()
        {
            new()
            {
                Amount              = 10,
                ForceMagnitude      = 1,
                Effects             = new(),
            }
        };

        Hitboxes = new()
        {
            new()
            {
                Form                = new()
                {
                    Prefab          = "HB_SwordSwing",
                    Offset          = new(){ x = 0, y = 0 },

                },
                Behavior            = new()
                {
                    Type            = HitboxBehavior.Attached,
                    AllowMultiHit   = false,
                },
                Direction           = new()
                {
                    Scope           = HitboxDirectionScope.Intercardinal
                },
                Lifetime            = new()
                {
                    FrameStart      = 1,
                    FrameEnd        = 20,
                    Phase           = WeaponPhase.Fire,
                },
            },
        };
        Animations = new()
        {
            OnFire = HeroAnimation.SwordStrike,
        };
    }
}

public class SwordRend : DamagingWeapon
{
    public SwordRend()
    {
        Name                        = "SwordRend";
        Trigger                     = new() { Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.OnPhase;
        LockTriggerAction           = true;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 3;
        FireDurationFrames          = 20;
        ControlWindow               = 0.3f;
        CanCancelDisables           = true;
        SwapOnFire                  = "null";
        ForceReleaseOnSwap          = true;
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "SwordSwingDisable",
                Cancelable          = false,
                DurationFrames      = 20,
                DisableAttack       = true,
                DisableMove         = true,                
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Cancelable          = true,
                DurationFrames      = 60,
                DisableAttack       = true,
                DisableMove         = true,                
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            }

        };

        MovementDefinitions = new()
        {
            new()
            {
                KinematicAction     = KinematicAction.Lunge,
                Speed               = 5,
                SpeedCurve          = new(new(0f, 1f, 0f, -0.25f), new(1f, .25f, 0f, 0f)),
                DurationFrames      = 15,
                PersistPastScope    = true,
                Phase               = WeaponPhase.Fire,
                Scope               = (int)WeaponPhase.Fire,
            }
        };

        DamageComponents    = new()
        {
            new()
            {
                Amount              = 10,
                ForceMagnitude      = 1,
                Effects             = new(),
            }
        };

        Hitboxes = new()
        {
            new()
            {
                Form                = new()
                {
                    Prefab          = "HB_SwordSwing",
                    Offset          = new(){ x = 0, y = 0 },

                },
                Behavior            = new()
                {
                    Type            = HitboxBehavior.Attached,
                    AllowMultiHit   = false,
                },
                Direction           = new()
                {
                    Scope           = HitboxDirectionScope.Intercardinal
                },
                Lifetime            = new()
                {
                    FrameStart      = 1,
                    FrameEnd        = 20,
                    Phase           = WeaponPhase.Fire,
                },
            },
        };
        Animations = new()
        {
            OnFire = HeroAnimation.SwordStrike,
        };
    }
}
