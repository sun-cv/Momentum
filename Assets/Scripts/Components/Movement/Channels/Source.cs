using System;
using UnityEngine;

namespace Momentum
{
    public class SourceChannel : IMovementChannel
    {
        private IMovementEngineConfig engine;

        private Context             context;
        private MovementIntent      intent;
        private IAttributeSystem    attribute;

        private StateFlag active   = new();

        public SourceChannel(ChannelConfig config)
        {
            this.engine     = config.engine;
            this.context    = config.context;
            this.intent     = config.intent;
            this.attribute  = context.component.attribute.System;

            active.Set();
        }

        public void Enter(MovementChannelParams param) {}

        public Vector2 GetVelocity()
        {
            if (engine == null) return Vector2.zero;

            Vector2 dir = intent.direction;
            float speed = Math.Clamp(attribute.Get<IMovementAttribute>().MovementSpeed, 0, engine.MaxMovementSpeed);

            Logwin.Log("Movement speed", speed, "Movement engine");
            Logwin.Log("Movement dir", dir, "Movement engine");

            return dir * speed;
        }

        public int  Priority        => 0;
        public bool IsActive        => active;
        public bool IgnoreFriction  => false;

        public void RequestCancel() {}
        public void RequestExit()   {}
        public void Exit()          {}
        public void Cancel()        {}
        public void TickFixed()     {}
    }
}
