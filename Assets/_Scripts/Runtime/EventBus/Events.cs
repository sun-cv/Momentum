using UnityEngine;


namespace Momentum
{

    public struct Tick      : IEvent {}
    public struct TickFixed : IEvent {}
    public struct TickLate  : IEvent {}

    public struct MouseClickLeft        : IEvent {}
    public struct MouseClickRight       : IEvent {}
    public struct MouseClickLeftCancel  : IEvent {}
    public struct MouseClickRightCancel : IEvent {}
    public struct MouseMovePosition     : IEvent
    {
        public Vector2 position;
    }
    public struct MoveFacing            : IEvent
    {
        public PrincipalDirection facing;
    }
    public struct MoveDirection         : IEvent
    {
        public Vector2 direction;
    }
    public struct DashInput             : IEvent {}
}