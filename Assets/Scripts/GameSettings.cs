



public static class Config
{
    public const float TICK_RATE_TICK           = 60;
    public const float TICK_RATE_LOOP           = 20;
    public const float TICK_RATE_STEP           = 10;
    public const float TICK_RATE_UTIL           = 5;

    public const float INPUT_THRESHOLD_RELEASE  = 40;
    public const float COMMAND_BUFFER_EXPIRY    = 10;

    public const float MOVEMENT_GROUND_FRICTION = 50f;
    public const float MOVEMENT_MAX_SPEED       = 20f;
    public const float MOVEMENT_ACCELERATION    = 100f;
    public const float MOVEMENT_DECELERATION    = 200;
    public const float MOVEMENT_IMPULSE_DECAY   = 10f;
}

public static class Settings
{

}



public static class ServiceUpdatePriority
{
    public static UpdatePriority TimerManager       = new(UpdatePhase.System,    10);
    public static UpdatePriority DevEnv             = new(UpdatePhase.System,    50);

    public static UpdatePriority InputRouter        = new(UpdatePhase.Input,     10);


    public static UpdatePriority Stats              = new(UpdatePhase.PreUpdate, 20);
    public static UpdatePriority CommandSystem      = new(UpdatePhase.PreUpdate, 30);
    public static UpdatePriority WeaponLogic        = new(UpdatePhase.PreUpdate, 40);
    public static UpdatePriority EffectManager      = new(UpdatePhase.PreUpdate, 50);

    public static UpdatePriority MovementEngine     = new(UpdatePhase.Update,    10);

}