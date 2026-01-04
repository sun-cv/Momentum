



public static class Config
{
    public const float TICK_RATE_TICK           = 60;
    public const float TICK_RATE_LOOP           = 20;
    public const float TICK_RATE_STEP           = 10;
    public const float TICK_RATE_UTIL           = 5;

    public const int   GRAPHICS_PPU             = 16;
    public const float GRAPHICS_ORTHOGRAPHIC    = 270f/32f;

    public const float INPUT_THRESHOLD_RELEASE  = 40;
    public const float COMMAND_BUFFER_EXPIRY    = 10;

    public const float MOVEMENT_MAX_SPEED       = 25f;
    public const float MOVEMENT_ACCELERATION    = 200f;
    public const float MOVEMENT_FRICTION        = 5f;
    public const float MOVEMENT_IMPULSE_DECAY   = 10f;
}

public static class Settings
{

}



public static class ServiceUpdatePriority
{
    public static UpdatePriority TimerManager       = new(UpdatePhase.System,    10);
    public static UpdatePriority SystemLoop         = new(UpdatePhase.System,    20);
    public static UpdatePriority DevEnv             = new(UpdatePhase.System,    50);

    public static UpdatePriority InputDriver        = new(UpdatePhase.Input,     10);
    public static UpdatePriority InputRouter        = new(UpdatePhase.Input,     20);


    public static UpdatePriority Stats              = new(UpdatePhase.Logic, 20);
    public static UpdatePriority CommandSystem      = new(UpdatePhase.Logic, 30);
    public static UpdatePriority WeaponLogic        = new(UpdatePhase.Logic, 40);
    public static UpdatePriority EffectManager      = new(UpdatePhase.Logic, 50);
    public static UpdatePriority MovementEngine     = new(UpdatePhase.Logic, 60);

    public static UpdatePriority PhysicsSimulation  = new(UpdatePhase.Physics,   10);



    public static UpdatePriority CameraRig          = new(UpdatePhase.Render, 10);

}