using System;
using UnityEngine;

namespace Momentum
{
    public class SourceChannel : IMovementChannel
    {
        private Rigidbody2D body;
        private Attribute attribute;
        private IMovementEngineConfig config;

        private StateFlag active   = new();

        public SourceChannel(Rigidbody2D body)
        {
            this.body = body;

            // REWORK REQUIRED - pull attribute from component; 
            // 


            // attribute   = context.attribute;
            active.Set();
        }

        public void Enter(MovementChannelParams param) {}

        public Vector2 GetVelocity()
        {
            if (config == null) return Vector2.zero;

            // Vector2 dir = context.movement.direction;
            // float speed = Math.Clamp(attribute.movement.Speed, 0, config.MaxSpeedInput);

            // return dir * speed;
            return Vector2.zero;
        }

        public int  Priority        => 0;
        public bool IsActive        => active;
        public bool IgnoreFriction  => false;

        public void RequestCancel() {}
        public void RequestExit() {}
        public void Exit() { }
        public void Cancel() { }
        public void TickFixed() { }
    }
}
