using UnityEngine;



public static class Config
{
    public static class Timing
    {
        public const float TICK_RATE_TICK           = 60f;
        public const float TICK_RATE_LOOP           = 30f;
        public const float TICK_RATE_STEP           = 15f;
        public const float TICK_RATE_UTIL           = 5f;
    }

    public static class Graphics
    {
        public const int   PPU                      = 16;
        public const float ORTHOGRAPHIC_SIZE        = 270f/(PPU * 2f);
    }

    public static class Input
    {
        public const float RELEASE_THRESHOLD        = 300f;
        public const float BUFFER_WINDOW_FRAMES     = 15f;

        public const float INTENT_DEFAULT           = 45f;
        public const float INTENT_HORIZONTAL        = 50f;
        public const float INTENT_VERTICAL          = 40f;
    }

    public static class Rendering
    {
        public const float SPRITE_OVERLAP_LOOKAHEAD = 0.1f;
    }

}


public static class Settings
{
    public static class Movement
    {
        public const float MAX_SPEED                        = 25f;
        public const float ACCELERATION                     = 25f;
        public const float FRICTION                         = 2.5f;
        public const float INERTIA                          = .2f;
        public const float FORCE_THRESHOLD                  = 2f;

        public static float FACING_SWITCH_DELAY             = 1f;
        public static float FACING_CLOCKWISE                = 0.1f;
        public static float FACING_COUNTER_CLOCKWISE        = 0.8f; 

        public static bool NORMALIZE_VELOCITY               = false;
    }   

    public static class Physics
    {
        public const float FRICTION                         = 5f;
    }

    public static class Debug   
    {   
        public static bool SHOW_HITBOXES                    = true;
        public static float GIZMO_ALPHA                     = 0.2f;
        public static Color GIZMO_COLOR                     = Color.red;
        public static Color GIZMO_COLOR_HITBOX              = Color.red;
        public static Color GIZMO_COLOR_SPAWNER             = new Color32(65, 224, 65, 255);
        public static Color GIZMO_COLOR_SPAWNER_SELECTED    = Color.blue;
    }

}


public static class ServiceUpdatePriority
{
    public static UpdatePriority TimerManager               = new(UpdatePhase.System,   10);
    public static UpdatePriority LoggingSystem              = new(UpdatePhase.System,   20);
    public static UpdatePriority SystemTick                 = new(UpdatePhase.System,   30);
    public static UpdatePriority SystemLoop                 = new(UpdatePhase.System,   31);
    public static UpdatePriority Services                   = new(UpdatePhase.System,   40);
    public static UpdatePriority DevEnv                     = new(UpdatePhase.System,   50);

    public static UpdatePriority InputDriver                = new(UpdatePhase.Input,    10);
    public static UpdatePriority InputRouter                = new(UpdatePhase.Input,    20);

    public static UpdatePriority EcoSystem                  = new(UpdatePhase.Logic,    01);
    public static UpdatePriority SpawnerService             = new(UpdatePhase.Logic,    02);
    public static UpdatePriority Spawner                    = new(UpdatePhase.Logic,    03);
    public static UpdatePriority TeleportService            = new(UpdatePhase.Logic,    04);
    public static UpdatePriority Stats                      = new(UpdatePhase.Logic,    10);
    public static UpdatePriority IntentSystem               = new(UpdatePhase.Logic,    20);
    public static UpdatePriority DirectionHandler           = new(UpdatePhase.Logic,    21);
    public static UpdatePriority CommandSystem              = new(UpdatePhase.Logic,    22);
    public static UpdatePriority InputIntent                = new(UpdatePhase.Logic,    23);
    public static UpdatePriority WeaponLogic                = new(UpdatePhase.Logic,    30);
    public static UpdatePriority EffectRegister             = new(UpdatePhase.Logic,    35);

    public static UpdatePriority HitboxManager              = new(UpdatePhase.Logic,    50);
    public static UpdatePriority TriggerCoordinator         = new(UpdatePhase.Logic,    55);  
    public static UpdatePriority DamageProcessor            = new(UpdatePhase.Logic,    60);
    public static UpdatePriority ParrySystem                = new(UpdatePhase.Logic,    61);
    public static UpdatePriority DamageCalculator           = new(UpdatePhase.Logic,    62);
    public static UpdatePriority DamageResolver             = new(UpdatePhase.Logic,    63);
    public static UpdatePriority Damage                     = new(UpdatePhase.Logic,    65);

    public static UpdatePriority CollisionHandler           = new(UpdatePhase.Physics,  10);
    public static UpdatePriority PhysicsEngine              = new(UpdatePhase.Physics,  20);
    public static UpdatePriority Movement                   = new(UpdatePhase.Physics,  25);
    public static UpdatePriority MovementEngine             = new(UpdatePhase.Physics,  30);

    public static UpdatePriority Resources                  = new(UpdatePhase.Resolve,  10);
    public static UpdatePriority Lifecycle                  = new(UpdatePhase.Resolve,  15);
    public static UpdatePriority Presence                   = new(UpdatePhase.Resolve,  20);
    public static UpdatePriority CorpseService              = new(UpdatePhase.Resolve,  25);
    public static UpdatePriority Corpse                     = new(UpdatePhase.Resolve,  30);

    public static UpdatePriority SpriteLayering             = new(UpdatePhase.Render,   10);
    public static UpdatePriority SpriteDepthSorting         = new(UpdatePhase.Render,   20);
    public static UpdatePriority AnimationSystem            = new(UpdatePhase.Render,   30);
    public static UpdatePriority AnimationController        = new(UpdatePhase.Render,   40);
    public static UpdatePriority CameraRig                  = new(UpdatePhase.Render,   50);
}
