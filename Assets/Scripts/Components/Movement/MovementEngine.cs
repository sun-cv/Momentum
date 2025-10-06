using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Momentum
{

    public enum MovementChannel 
    {
        Source,
        Modified,
        Controlled,
        Impulse
    }

    public struct ChannelConfig
    {
        public Context context;
        public MovementIntent intent;
        public IMovementEngineConfig engine;
    }

    public class MovementEngine : IMovementEngine
    {
        private IMovementEngineConfig  engineConfig;
        private ChannelConfig          channelConfig;

        private readonly Dictionary<MovementChannel, IMovementChannel> channels = new();
        private IMovementChannel source;
        private IMovementChannel modified;
        private IMovementChannel controlled;
        private IMovementChannel impulse;

        private IMovementChannel active;

        private Context context;
        private Rigidbody2D body;
        private MovementIntent movementIntent;

        private Vector2 momentum;
        private Vector2 velocity;

        private bool frictionEnabledThisFrame   = true;

        // API
        public void EnterChannel(MovementChannel selection, MovementChannelParams param) => channels[selection].Enter(param);
        public void RequestExit()   => active.RequestExit();
        public void RequestCancel() => active.RequestCancel();

        public void BindBody(Rigidbody2D body)              => this.body            = body;
        public void BindConfig(MovementEngineConfig config) => this.engineConfig    = config;
        public void BindContext(Context context)            => this.context         = context;
        public void BindIntent(MovementIntent intent)       => this.movementIntent  = intent;

        public void Initialize()
        {
            body.freezeRotation = true;
            body.gravityScale   = 0;

            channelConfig = new ChannelConfig(){ context = context, intent = movementIntent, engine = engineConfig };

            CreateChannels();
        }

        public void TickFixed()
        {
            Logwin.Log("intent", movementIntent.direction, "Movement engine");

            SetActive();
            SetVelocity();

            foreach (var (key, channel) in channels) if (channel.IsActive) channel.TickFixed();

            ApplyMomentum();
            ApplyFrictionIfNeeded();

            body.linearVelocity = velocity;
        }

        void ApplyMomentum()
        {
            velocity += momentum;

            float friction = frictionEnabledThisFrame ? engineConfig.GroundFriction : 0f;

            if (friction > 0f) 
            {
                momentum = Vector2.MoveTowards(momentum, Vector2.zero, friction * Time.fixedDeltaTime);
            }
            momentum = Vector2.ClampMagnitude(momentum, engineConfig.MaxMomentum);
        }

        void ApplyFrictionIfNeeded()
        {
            frictionEnabledThisFrame = !active.IgnoreFriction;

            if (!frictionEnabledThisFrame) return;

            bool noDriver = !channels.Any(pair => pair.Value.IsActive);

            if (noDriver)
            {
                velocity = Vector2.MoveTowards(velocity, Vector2.zero, engineConfig.GroundFriction * Time.fixedDeltaTime);
            }
        }
        
        void SetActive()    => active   = channels.Where(channel => channel.Value.IsActive).OrderByDescending(channel => channel.Value.Priority).FirstOrDefault().Value ?? source;
        void SetVelocity()  => velocity = active?.GetVelocity() ?? Vector2.zero;
        
        void CreateChannels()
        {
            source         = new SourceChannel(channelConfig);
            modified       = new ModifiedChannel(channelConfig);
            controlled     = new ControlledChannel(channelConfig);
            impulse        = new ImpulseChannel(channelConfig);

            channels.Add(MovementChannel.Source,      source);
            channels.Add(MovementChannel.Modified,    modified);
            channels.Add(MovementChannel.Controlled,  controlled);
            channels.Add(MovementChannel.Impulse,     impulse);
        }
    }
}