using UnityEngine;


namespace Momentum
{

        // Attribute component? 
        // direct get component call in on awake pull movement attribute?
        // public void SetMovementAttributes(); // REWORK REQUIRED FOR ATTRIBUTE FORM ?

    public class MovementIntent
    {
        public Vector2 direction = Vector2.zero;
    }

    public interface IMovement
    {
        public void BindMovementIntent(MovementIntent intent);
    
        public void StartSource(MovementChannelParams param);
        public void StartModified(MovementChannelParams param);
        public void StartControlled(MovementChannelParams param);
        public void StartImpulse(MovementChannelParams param);

        public void RequestChannelExit();
        public void RequestChannelCancel();
    }

    public interface IMovementChannel
    {
        void Enter(MovementChannelParams config = null);
        void RequestExit();
        void RequestCancel();
        void TickFixed();
        Vector2 GetVelocity();

        int  Priority           { get; }   
        bool IsActive           { get; }
        bool IgnoreFriction     { get; }
    }

    public class MovementChannelParams {}

}