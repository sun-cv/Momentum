


public class Dash : Weapon
{
    public Dash()
    {
        SlotType                    = EquipmentSlotType.Dash;
        Definition                  = new DashDefinition(); 
    }
}



public class DashDefinition : WeaponDefinition
{

    public DashDefinition()
    {
        Name                        = "BaseDash";
        actions = new()
        {
            { "BaseDash",           new BaseDash()   },
        };
    }
}

// ============================================================================
// DASH
// ============================================================================

public class BaseDash : MovementWeapon
{
    public BaseDash()
    {
        Name                        = "Dash";
        DefaultWeapon               = Capability.Dash;
        Trigger                     = new() { Capability.Dash };
        Activation                  = WeaponActivation.OnPress;
        Termination                 = WeaponTermination.AfterFire;
        Availability                = WeaponAvailability.Default;
        AcceptTriggerLockRequests   = true;
        ChargeTimeFrames            = 3;
        FireDurationFrames          = 10;
        Cooldown                    = 0.5f;
        CanCancelDisables           = true;
        CanInterrupt                = true;
        Effects = new()
        {
            new DashDisable()
            {   
                Name                = "DashDisable",
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 30,
                DisableRotate       = true,
                RequestActionLock   = true,
                ActionLocks         = new(){ Capability.Dash }
            },

            new DashDisable()
            {
                Name                = "DashDisableAttack",
                Active              = true,
                Cancelable          = false,
                DisableAttack       = true,
                DurationFrames      = 15,
                RequestActionLock   = true,
            },

            new WeaponMobility()
            {
                Trigger             = WeaponPhase.FireEnd,
                Name                = "DashMobilityDisable",
                Type                = EffectType.Speed,
                Active              = true,
                Cancelable          = false,
                DurationFrames      = 15,
                Modifier            = 0.5f,
            },
        };
    }
}



// public class BaseDash : MovementWeapon
// {
//     public BaseDash()
//     {
//         Name                        = "Dash";
//         DefaultWeapon               = Capability.Dash;
//         Trigger                     = new() { Capability.Dash };
//         Activation                  = WeaponActivation.OnPress;
//         Termination                 = WeaponTermination.AfterFire;
//         Availability                = WeaponAvailability.Default;
//         AcceptTriggerLockRequests   = true;
//         ChargeTimeFrames            = 3;
//         FireDurationFrames          = 10;
//         Cooldown                    = 0.5f;
//         WeaponOverridesMovement     = true;
//         Velocity                    = 15;
//         Modifier                    = 1f;
//         LockDirection               = true;
//         CanCancelDisables           = true;
//         CanInterrupt                = true;
//         Effects = new()
//         {
//             new DashDisable()
//             {   
//                 Name                = "DashDisable",
//                 Active              = true,
//                 Cancelable          = false,
//                 DurationFrames      = 30,
//                 DisableRotate       = true,
//                 RequestActionLock   = true,
//                 ActionLocks         = new(){ Capability.Dash }
//             },

//             new DashDisable()
//             {
//                 Name                = "DashDisableAttack",
//                 Active              = true,
//                 Cancelable          = false,
//                 DisableAttack       = true,
//                 DurationFrames      = 15,
//                 RequestActionLock   = true,
//             },

//             new WeaponMobility()
//             {
//                 Trigger             = WeaponPhase.FireEnd,
//                 Name                = "DashMobilityDisable",
//                 Type                = EffectType.Speed,
//                 Active              = true,
//                 Cancelable          = false,
//                 DurationFrames      = 15,
//                 Modifier            = 0.5f,
//             },

//             new WeaponMobility()
//             {
//                 Trigger             = WeaponPhase.Charging,
//                 Name                = "DashMobilitySlow",
//                 Type                = EffectType.Grip,
//                 Active              = true,
//                 Cancelable          = false,
//                 DurationFrames      = 30,
//                 Modifier            = 1f,
//                 ModifierTarget      = 0f,
//                 ModifierSpeed       = .5f,
//             },
//         };
//     }
// }