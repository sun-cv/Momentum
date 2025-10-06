using UnityEngine;

namespace Momentum
{
    public class ModifiedChannel : IMovementChannel
    {
        private int priority                = 25;

        private IMovementEngineConfig engine;
        private ModifiedChannelParams config;

        private Context             context;
        private MovementIntent      intent;
        private IAttributeSystem    attribute;
        private Rigidbody2D         body;

        private StateFlag active            = new();
        private StateFlag ignoreFriction    = new();

        private RequestFlag requestCancel   = new();
        private RequestFlag requestExit     = new();

        public ModifiedChannel(ChannelConfig config)
        {
            this.engine     = config.engine;
            this.context    = config.context;
            this.intent     = config.intent;
            this.body       = config.context.entity.body.rigidBody;
            this.attribute  = context.component.attribute.System;
        }


        public void Enter(MovementChannelParams param)
        {
            ClearStaleRequests();

            config = (ModifiedChannelParams)param;

            SetIgnoreFriction();

            active.Set();
        }

        public void TickFixed() 
        {
            if (requestCancel.Consume())    Cancel();
            if (requestExit.Consume())      Exit();
        }        

        public void Cancel() => Exit();
        public void Exit()
        {
            active.Clear();
        }



        public Vector2 GetVelocity()
        {
            if (config == null) return Vector2.zero;

            Vector2 velocity = body.linearVelocity;

            velocity *= config.SpeedMultiplier;

            if (config.OverrideDirection != Vector2.zero)
            {
                velocity = config.OverrideDirection.normalized * velocity.magnitude;
            }

            if (config.MaxSpeed > 0f)
            {
                velocity = Vector2.ClampMagnitude(velocity, config.MaxSpeed);
            }

            return velocity;
        }

        public void ClearStaleRequests() { requestCancel.Consume(); requestExit.Consume(); }
        public void RequestCancel() => requestCancel.Set();
        public void RequestExit()   => requestExit.Set();

        void SetIgnoreFriction()    { if (config.IgnoreFriction) { ignoreFriction.Set(); } else { ignoreFriction.Clear();}}

        public int  Priority        => priority;
        public bool IsActive        => active;
        public bool IgnoreFriction  => ignoreFriction;

    }
}
