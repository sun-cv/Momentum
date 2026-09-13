using UnityEngine;



namespace Game.Common
{
    public readonly struct InputEvent : IEvent
    { 
        public Capability Capability    { get; init; }
        public bool Pressed             { get; init; }
        public bool Released            { get; init; }
    }

    public readonly struct AimVector : IEvent
    {
        public Vector2 Vector           { get; init; }
    }

    public readonly struct IntentVector : IEvent
    {
        public Vector2 Vector           { get; init; }
    }
}
