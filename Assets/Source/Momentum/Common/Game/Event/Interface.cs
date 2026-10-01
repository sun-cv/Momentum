using UnityEngine;



namespace Game.Common
{
    public readonly struct InputEvent : IEvent
    { 
        public Capability Capability    { get; init; }
        public bool Pressed             { get; init; }
        public bool Released            { get; init; }
    }

    
    public readonly struct MousePosition : IEvent
    {
        public Vector2 World            { get; init; }
        public Vector2 Screen           { get; init; }
    }


    public readonly struct MovementIntent : IEvent
    {
        public Vector2 Vector           { get; init; }
    }
}
