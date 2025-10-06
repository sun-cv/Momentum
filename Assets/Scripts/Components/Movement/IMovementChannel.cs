using UnityEngine;


namespace Momentum
{

    public interface IMovementChannel : ITickFixed
    {
        void Enter(MovementChannelParams config = null);
        void RequestExit();
        void RequestCancel();
        Vector2 GetVelocity();

        int  Priority           { get; }   
        bool IsActive           { get; }
        bool IgnoreFriction     { get; }
    }
}