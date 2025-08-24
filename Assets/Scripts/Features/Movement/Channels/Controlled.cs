using UnityEngine;


namespace Momentum
{


    public class ControlledChannel : IMovementChannel
    {
        private Rigidbody2D body;
        private ControlledChannelParams config;

        private StateFlag  active          = new();
        private StateFlag  ignoreFriction  = new();

        private RequestFlag requestCancel   = new();
        private RequestFlag requestExit     = new();

        private Vector2     startMomentum;
        private Stopwatch   elapsed         = new();

        public ControlledChannel(Rigidbody2D body)
        {
            this.body = body;
        }

        public void Enter(MovementChannelParams param)
        {
            ClearStaleRequests();
            config = (ControlledChannelParams)param;

            SetStartingMomentum();
            SetIgnoreFriction();

            elapsed.Start();
            active.Set();
        }

        public void TickFixed() 
        {
            if (requestCancel.Consume())    Cancel();
            if (requestExit.Consume())      Exit();
        }        

        public void Cancel()
        {
            startMomentum = Vector2.zero;
            elapsed.Stop().Reset();
            active.Clear();
        }

        public void Exit()
        {
            elapsed.Stop().Reset();
            active.Clear();
        }


        public Vector2 GetVelocity()
        {
            float time      = Mathf.Clamp01(elapsed.CurrentTime / config.duration);
            float prevTime  = Mathf.Clamp01((elapsed.CurrentTime - Time.fixedDeltaTime) / config.duration);

            Vector2 velocity = Vector2.zero;

            switch(config.Mode)
            {
                case ControlledMovementMode.Distance:
                {
                    float positionNow   = config.movementCurve.Evaluate(time);
                    float positionPrev  = config.movementCurve.Evaluate(prevTime);

                    float deltaDistance = (positionNow - positionPrev) * config.distance / Time.fixedDeltaTime;
                    velocity            = config.direction.normalized * deltaDistance;
                }
                    break;

                case ControlledMovementMode.Force:
                {
                    float baseSpeed = config.force / config.duration;
                    float speed     = baseSpeed * Mathf.Max(0f, config.movementCurve.Evaluate(time));
                    velocity        = config.direction.normalized * speed;
                }
                    break;
            }

            if (startMomentum != Vector2.zero)
            {
                float blend  = Mathf.Clamp01(config.momentumBlendCurve.Evaluate(time));
                velocity    += startMomentum * blend;
            }

            return velocity;
        }

        void SetStartingMomentum()  => startMomentum  = config.inheritMomentum ? body.linearVelocity : Vector2.zero;
        void SetIgnoreFriction()    { if (config.ignoreFriction) { ignoreFriction.Set(); } else { ignoreFriction.Clear();}}

        public int  Priority        => 50;
        public bool IsActive        => active;
        public bool IgnoreFriction  => ignoreFriction;

        public void RequestCancel() => requestCancel.Set();
        public void RequestExit()   => requestExit.Set();
        public void ClearStaleRequests() { requestCancel.Consume(); requestExit.Consume(); }
    }
}