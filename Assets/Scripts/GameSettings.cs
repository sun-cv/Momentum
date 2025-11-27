

using UnityEngine.InputSystem.Composites;

public static class GameSettings
{
    public const float TICK_RATE_TICK = 60;
    public const float TICK_RATE_LOOP = 20;
    public const float TICK_RATE_STEP = 10;
    public const float TICK_RATE_UTIL = 5;


    public const float INPUT_THRESHOLD_HOLD                     = 10;
    public const float INPUT_THRESHOLD_EXPIRY_RECENTLY_RELEASED = 20;
    public const float INPUT_THRESHOLD_BUFFER_RELEASE           = 20;
    public const float INPUT_THRESHOLD_BUFFER_MINIMUM_HOLD      = 20;
}
