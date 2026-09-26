using UnityEngine;



namespace Game.Common.Events
{

    public readonly struct CameraCreated : IEvent
    {
        public Camera View                  { get; init; }
    }
}
