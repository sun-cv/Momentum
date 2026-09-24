using UnityEngine;



namespace Game.Common.Events
{
    public readonly struct BodyContacted    : IEvent
    {
        public Entity Source    { get; init; }
        public Entity Target    { get; init; }
        public Vector2 Normal   { get; init; }
    }

    public readonly struct SurfaceContacted : IEvent
    {
        public Entity Source    { get; init; }
        public Vector2 Normal   { get; init; }
    }
}
