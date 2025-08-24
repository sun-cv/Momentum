using UnityEngine;

namespace Momentum
{
    public class ImpulseChannel : IMovementChannel
    {
        private Rigidbody2D body;
        private ImpulseChannelParams config;

        private StateFlag active   = new();
        private Stopwatch elapsed   = new();

        private RequestFlag requestCancel   = new();
        private RequestFlag requestExit     = new();

        public ImpulseChannel(Rigidbody2D body)
        {
            this.body = body;
        }

        public void Enter(MovementChannelParams param)
        {
            ClearStaleRequests();

            config = (ImpulseChannelParams)param;

            elapsed.Start();
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
            elapsed.Stop().Reset();
            active.Clear();
        }


        public Vector2 GetVelocity()
        {
            if (!active || config == null)
                return Vector2.zero;

            float time          = Mathf.Clamp01(elapsed.CurrentTime / config.Duration);
            float strength      = config.ForceCurve.Evaluate(time) * config.Force;
            Vector2 velocity    = config.Direction.normalized * strength;

            if (time >= 1f) Exit();

            return velocity;
        }

        public void RequestCancel() => requestCancel.Set();
        public void RequestExit()   => requestExit.Set();
        public void ClearStaleRequests() { requestCancel.Consume(); requestExit.Consume(); }

        public int  Priority        => 40;
        public bool IsActive        => active;
        public bool IgnoreFriction  => false;
    }
}
