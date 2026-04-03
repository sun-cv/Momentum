


public class Corpse : Actor, ICorpse
{
    public Presence             Presence            { get; set; }
    public Decomposition        Decomposition       { get; set; }
    public Resources            Resource            { get; set; }
    public AnimationSystem      Animation           { get; set; }

    public Decomposition.State Condition            => Decomposition.Condition;
    public float Integrity                          => Resource.Integrity;
    public float MaxIntegrity                       => Definition.Stats.MaxIntegrity;

    public void Initialize(ActorDefinition definition)
    {
        Definition      = definition;

        Presence        = new(this);
        Decomposition   = new(this);
        Resource        = new(this);
        Animation       = new(this);
    }
}
