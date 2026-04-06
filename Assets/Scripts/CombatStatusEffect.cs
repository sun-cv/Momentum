















public struct Status
{
    public StatusEffect Effect  { get; set; }
    public float Duration       { get; set; }

    public Status(StatusEffect effect, float duration)
    {
        Effect      = effect;
        Duration    = duration;
    }

}


public struct StatusConfig
{
    public float Chance                { get; set; }
    public Damage Damage               { get; set; }

    public StatusConfig(float chance, Damage damage)
    {
        Chance = chance;
        Damage = damage;
    }
}

public struct StatusComponent
{
    public Status Status        { get; set; }
    public StatusConfig Config  { get; set; } 

    public StatusComponent(Status status, StatusConfig config)
    {
        Status  = status;
        Config  = config;
    }
}    












public enum StatusEffect
{
    Frozen,
    Burning,
    Stunned,
}
