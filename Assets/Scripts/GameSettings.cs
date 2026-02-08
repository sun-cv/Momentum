




using UnityEngine;

public static class Config
{
    public static class Timing
    {
        public const float TICK_RATE_TICK           = 60f;
        public const float TICK_RATE_LOOP           = 20f;
        public const float TICK_RATE_STEP           = 10f;
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
        public const float MAX_SPEED                    = 25f;
        public const float ACCELERATION                 = 25f;
        public const float FRICTION                     = 5f;
        public const float MOMENTUM_RETENTION           = 0f;
        public const float INERTIA                      = .2f;
        public const float FORCE_THRESHOLD              = 2f;

        public static float FACING_SWITCH_DELAY         = 1f;
        public static float FACING_CLOCKWISE            = 0.1f;
        public static float FACING_COUNTER_CLOCKWISE    = 0.8f; 

        public static bool NORMALIZE_VELOCITY           = false;
    }

    public static class Debug
    {
        public static bool SHOW_HITBOXES                = true;
        public static Color GIZMO_COLOR                 = Color.red;
    }

}



public static class ServiceUpdatePriority
{
    public static UpdatePriority TimerManager       = new(UpdatePhase.System,   10);
    public static UpdatePriority SystemLoop         = new(UpdatePhase.System,   20);
    public static UpdatePriority DevEnv             = new(UpdatePhase.System,   50);

    public static UpdatePriority InputDriver        = new(UpdatePhase.Input,    10);
    public static UpdatePriority InputRouter        = new(UpdatePhase.Input,    20);

    public static UpdatePriority IntentSystem       = new(UpdatePhase.Logic,    10);
    public static UpdatePriority Stats              = new(UpdatePhase.Logic,    20);
    public static UpdatePriority CommandSystem      = new(UpdatePhase.Logic,    30);
    public static UpdatePriority WeaponLogic        = new(UpdatePhase.Logic,    40);
    public static UpdatePriority EffectManager      = new(UpdatePhase.Logic,    50);
    public static UpdatePriority DirectionHandler   = new(UpdatePhase.Logic,    60);
    public static UpdatePriority MovementEngine     = new(UpdatePhase.Logic,    70);
    public static UpdatePriority Lifecycle          = new(UpdatePhase.Logic,    80);

    public static UpdatePriority HitboxManager      = new(UpdatePhase.Physics,  10);
    public static UpdatePriority SpriteDepthSorting = new(UpdatePhase.Physics,  90);

    public static UpdatePriority TriggerCoordinator = new(UpdatePhase.Resolve,  10);  
    public static UpdatePriority Combat             = new(UpdatePhase.Resolve,  20);

    public static UpdatePriority SpriteLayering     = new(UpdatePhase.Render,   10);
    public static UpdatePriority AnimationHandler   = new(UpdatePhase.Render,   20);
    public static UpdatePriority CameraRig          = new(UpdatePhase.Render,   30);

}