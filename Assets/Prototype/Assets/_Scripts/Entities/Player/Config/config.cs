using character.config;


namespace character
{


public class Config
{
    Combat   combat                 = new();
    Movement movement               = new();
}
}

namespace character.config
{
    
public class Combat
{

}

public class Movement
{
    public readonly Idle idle       = new();
    public readonly Dash dash       = new();
    public readonly Sprint sprint   = new();
    public readonly Facing facing   = new();

}

public class Idle
{
    public readonly float movementSpeed = 0;
}

public class Dash
{
    public readonly float speed         = 40;
    public readonly float duration      = 0.15f;
    public readonly float distance      = 25f;

    public readonly float charges       = 2.0f;
    public readonly float cooldown      = 1.0f;

    public float SpatialSpeed           => distance/duration;
}

public class Sprint
{
    public readonly float speed     = 10;
}

public class Facing
{
}
}