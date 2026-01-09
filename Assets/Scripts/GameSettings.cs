



public static class Config
{
    public static class Timing
    {
        public const float TICK_RATE_TICK           = 60;
        public const float TICK_RATE_LOOP           = 20;
        public const float TICK_RATE_STEP           = 10;
        public const float TICK_RATE_UTIL           = 5;
    }

    public static class Graphics
    {
        public const int   PPU                      = 16;
        public const float ORTHOGRAPHIC_SIZE        = 270f/(PPU * 2f);
    }

    public static class Input
    {
        public const float RELEASE_THRESHOLD        = 40;
        public const float COMMAND_BUFFER_EXPIRY    = 10;
    }
}

public static class Settings
{

    public static class Movement
    {
        public const float MAX_SPEED                = 25f;
        public const float ACCELERATION             = 200f;
        public const float FRICTION                 = 10f;
        public const float IMPULSE_DECAY            = 10f;
    }

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


    public static UpdatePriority CameraRig          = new(UpdatePhase.Render, 10);

}