using UnityEngine;
using character.context;


namespace character.controller
{


public class SprintControl : ActionControl
{
    private Rigidbody2D body;
    
    private Context context;
    private MovementContext movement;

    private float speedMax      = 10;
                        
    public SprintControl(Context _context)
    {
        context     = _context;
        movement    = context.Movement;
        body        = context.Core.Body;
    }


    public override void Start()
    {
        active.Set();
    }

    public override void Stop()
    {
        active.Clear();
    }


    public override void Tick()
    {
        if (!active)
        {
            return;
        }

        MoveCharacter();
    }

    private void MoveCharacter()
    {
        ApplyMovementForce();
    }


    private void ApplyMovementForce()
    {
        body.linearVelocity = speedMax * movement.Direction.normalized;
    }


}





}