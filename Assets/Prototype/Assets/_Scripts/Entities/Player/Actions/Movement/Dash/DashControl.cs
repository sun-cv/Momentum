using UnityEngine;
using character.context;


namespace character.controller
{


public class DashControl : ActionControl
{
    private Rigidbody2D body;
    
    private Context context;
    private MovementContext movement;


    private Vector2 dashDirection;
    private float speedMax      = 40;
                        

    public DashControl(Context _context)
    {
        context     = _context;
        movement    = context.Movement;
        body        = context.Core.Body;
    }

    public override void Start()
    {
        active.Set();

        dashDirection = context.Movement.Direction;
    }

    public override void Stop()
    {
        dashDirection = Vector2.zero;     

        active.Clear();
    }

    public override void TickFixed()
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
        body.linearVelocity = speedMax * dashDirection.normalized;
    }


}





}