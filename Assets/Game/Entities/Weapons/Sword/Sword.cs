

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

        Hitboxes = new()
        {
            new()
            {
                Prefab              = "HB_SwordSwing",
                Offset              = new(){ x = 0, y = 0 },
                Quaternion          = new(){ z = 0 },
                FrameStart          = 1,
                FrameEnd            = 60,
                Behavior            = HitboxBehavior.Attached,
                AllowMultiHit       = false,
            }
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
        AddControlOnFire            = new() { "SwordRend"};
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
                DurationFrames      = 30,
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
        Trigger                     = new() { Capability.Attack1 };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.OnPhase;
        LockTriggerAction           = true;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 3;
        FireDurationFrames          = 25;
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
                DurationFrames      = 60,
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
                DurationFrames      = 60,
                DisableAttack       = true,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Attack1 }
            }
        };
    }
}
