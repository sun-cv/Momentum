




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
        FireDurationFrames          = 20;
        ControlWindow               = 0.3f;
        AddControlOnFire            = new() { "SwordCleave"};
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
                DisableMove         = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 30,
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
                Action              = MovementAction.Lunge,
                Speed               = 4,
                SpeedCurve          = new(new(0f, 1f, 0f, -0.25f), new(1f, .25f, 0f, 0f)),
                DurationFrame       = 25,
                PersistPastScope    = true,
                Phase               = WeaponPhase.Fire,
            }
        };

        Hitboxes = new()
        {
            new()
            {
                Prefab              = "HB_SwordSwing",
                Offset              = new(){ x = 0, y = 0 },
                FrameStart          = 1,
                FrameEnd            = 20,
                Behavior            = HitboxBehavior.Attached,
                Phase               = WeaponPhase.Fire,
                AvailableDirections = HitboxDirection.Intercardinal,
                AllowMultiHit       = false,
            }
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
        FireDurationFrames          = 20;
        ControlWindow               = 0.3f;
        AddControlOnFire            = new() { "SwordRend"};
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
                DisableMove         = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            },
            new SwordSwingDisable()
            {
                Name                = "SwordSwingDisableCancelable",
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 30,
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
                Action              = MovementAction.Lunge,
                Speed               = 4,
                DurationFrame       = 25,
                PersistPastScope    = true,
                Phase               = WeaponPhase.Fire,
            }
        };

        Hitboxes = new()
        {
            new()
            {
                Prefab              = "HB_SwordSwing",
                Offset              = new(){ x = 0, y = 0 },
                FrameStart          = 1,
                FrameEnd            = 20,
                Behavior            = HitboxBehavior.Attached,
                Phase               = WeaponPhase.Fire,
                AvailableDirections = HitboxDirection.Intercardinal,          
                AllowMultiHit       = false,
            }
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
        SwapOnFire                  = "null";
        ForceReleaseOnSwap          = true;
        Effects = new()
        {
            new SwordSwingDisable()
            {   
                Name                = "SwordSwingDisable",
                Active              = true,
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
                Active              = true,
                Cancelable          = true,
                DurationFrames      = 30,
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
                Action              = MovementAction.Lunge,
                Speed               = 4,
                DurationFrame       = 25,
                PersistPastScope    = true,
                Phase               = WeaponPhase.Fire,
            }
        };

        Hitboxes = new()
        {
            new()
            {
                Prefab              = "HB_SwordSwing",
                Offset              = new(){ x = 0, y = 0 },
                FrameStart          = 1,
                FrameEnd            = 20,
                Behavior            = HitboxBehavior.Attached,
                Phase               = WeaponPhase.Fire, 
                AvailableDirections = HitboxDirection.Intercardinal,            
                AllowMultiHit       = false,
            }
        };
    }
}
