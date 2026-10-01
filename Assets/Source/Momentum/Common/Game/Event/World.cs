


namespace Game.Common.Events
{

    public readonly struct DealDamage : IEvent
    {
        public Entity Source        { get; init; }
        public Entity Target        { get; init; }
    }

    public readonly struct DealHealing : IEvent
    {

    }

    public readonly struct Heal : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct Wound : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct HealthChanged : IEvent
    {
        public Entity Entity        { get; init; }
        public int Before           { get; init; }
        public int After            { get; init; }
    }

    public readonly struct HealthThreshold : IEvent
    {
        public Entity Entity        { get; init; }
        public float Value          { get; init; }
    }

    public readonly struct Repair : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct Fracture : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct ArmorChanged : IEvent
    {
        public Entity Entity        { get; init; }
        public int Before           { get; init; }
        public int After            { get; init; }
    }

    public readonly struct Restore : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct Dissipate : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct ShieldChanged : IEvent
    {
        public Entity Entity        { get; init; }
        public int Before           { get; init; }
        public int After            { get; init; }
    }

    public readonly struct Recharge : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct Expend : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct EnergyChanged : IEvent
    {
        public Entity Entity        { get; init; }
        public int Before           { get; init; }
        public int After            { get; init; }
    }

    public readonly struct Deplete : IEvent
    {
        public Entity Entity        { get; init; }
        public int Amount           { get; init; }
    }

    public readonly struct IntegrityChanged : IEvent
    {
        public Entity Entity        { get; init; }
        public int Before           { get; init; }
        public int After            { get; init; }
    }

}
