


using UnityEngine;

namespace Momentum
{


    public enum ControlledMovementMode { Distance, Force }


    public class ControlledChannelParams : MovementChannelParams
    {
        public ControlledMovementMode Mode          = ControlledMovementMode.Distance;

        public float distance                       = 0f;
        public float duration                       = 0.2f;
        public float force                          = 0f;
        public Vector2 direction                    = Vector2.right;
    
        public bool inheritMomentum                 = false;
        public bool ignoreFriction                  = false;

        public AnimationCurve movementCurve         = AnimationCurve.Linear(0, 1, 1, 1);
        public AnimationCurve momentumBlendCurve    = AnimationCurve.Linear(0, 1, 1, 0); 


        public ControlledChannelParams FromAttack(Attack mechanic, AttackData data, Context context)
        {
            return new ControlledChannelParams(){
                Mode                    = mechanic.definition.mode,

                distance                = data.distance,
                duration                = data.duration,
                force                   = data.force,
                direction               = context.movement.lastDirection == Vector2.zero ? context.movement.defaultDirection : context.movement.lastDirection,
                
                inheritMomentum         = data.inheritMomentum,
                ignoreFriction          = data.ignoreFriction,
                movementCurve           = data.movementCurve,
                momentumBlendCurve      = data.momentumCurve,
            };
        }
        
        public ControlledChannelParams FromDash(Dash mechanic, DashData data, Context context)
        {
            return new ControlledChannelParams(){
                Mode                    = mechanic.definition.mode,

                distance                = context.attribute.dash.Distance,
                duration                = context.attribute.dash.Duration,
                force                   = context.attribute.dash.Force,
                direction               = context.movement.lastDirection == Vector2.zero ? context.movement.defaultDirection : context.movement.lastDirection,
                
                inheritMomentum         = data.inheritMomentum,
                ignoreFriction          = data.ignoreFriction,
                movementCurve           = data.movementCurve,
                momentumBlendCurve      = data.momentumCurve,
            };
        }
    }


}