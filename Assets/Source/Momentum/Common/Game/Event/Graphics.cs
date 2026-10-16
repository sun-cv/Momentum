using UnityEngine;



namespace Game.Common.Events
{

    public readonly struct CameraCreated : IEvent
    {
        public Camera View                  { get; init; }
    }

    public readonly struct CueCamera : IEvent
    {
        public Cue Cue                      { get; init; }
        public Vector2 Origin               { get; init; }
        public Entity? Source               { get; init; }
        public Entity? Owner                { get; init; }
    }

    public readonly struct StopCameraCue : IEvent
    {
        public string Id                    { get; init; }
        public Entity? Source               { get; init; }
        public Entity? Owner                { get; init; }
    }

    public readonly struct FrameCamera : IEvent
    {
        public Framing Framing              { get; init; }
        public string Id                    { get; init; }
        public Entity? Owner                { get; init; }
        public int In                       { get; init; }
        public int Out                      { get; init; }
    }

    public readonly struct StopFrameCamera : IEvent
    {
        public string Id                    { get; init; }
        public Entity? Owner                { get; init; }
    }

}
