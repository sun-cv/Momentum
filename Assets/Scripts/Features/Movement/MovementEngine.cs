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


    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementEngine : MonoBehaviour
    {
        [SerializeField]
        private IMovementEngineConfig  engineConfig;

        private readonly Dictionary<MovementChannel, IMovementChannel> channels = new();
        private IMovementChannel source;
        private IMovementChannel modified;
        private IMovementChannel controlled;
        private IMovementChannel impulse;

        private IMovementChannel active;

        private Rigidbody2D body;
        private MovementIntent movementIntent;

        private Vector2 momentum;
        private Vector2 velocity;

        private bool frictionEnabledThisFrame   = true;

        public void OnAwake()
        {
            body = GetComponent<Rigidbody2D>();

            body.freezeRotation = true;
            body.gravityScale   = 0;
        
            CreateChannels();
        }

        public void Update()
        {

        }

        public void UpdateFixed()
        {
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
            momentum = Vector2.ClampMagnitude(momentum, engineConfig.MomentumCap);
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
        

        public void StartChannel(MovementChannel select, MovementChannelParams param)
        {

        }

        public void RequestChannelExit()   => active.RequestExit();
        public void RequestChannelCancel() => active.RequestCancel();

        public void BindMovementIntent(MovementIntent intent) => movementIntent = intent;

        void SetActive()    => active   = channels.Where(channel => channel.Value.IsActive).OrderByDescending(channel => channel.Value.Priority).FirstOrDefault().Value;
        void SetVelocity()  => velocity = active?.GetVelocity() ?? Vector2.zero;
        
        void CreateChannels()
        {
            source         = new SourceChannel(body);
            modified       = new ModifiedChannel(body);
            controlled     = new ControlledChannel(body);
            impulse        = new ImpulseChannel(body);

            channels.Add(MovementChannel.Source,      source);
            channels.Add(MovementChannel.Modified,    modified);
            channels.Add(MovementChannel.Controlled,  controlled);
            channels.Add(MovementChannel.Impulse,     impulse);
        }
    }
}