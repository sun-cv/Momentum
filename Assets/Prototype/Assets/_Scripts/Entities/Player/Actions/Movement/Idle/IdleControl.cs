using UnityEngine;
using character.context;

namespace character.controller
{


public class IdleControl : ActionControl
{
    private Rigidbody2D                 body;
    
    private Context                     context;
    private MovementContext             movement;

    public IdleControl(Context _context)
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

    public override void TickFixed()
    {
        if (!active)
        {
            return;
        }
        ZeroVelocityOnIdle();  
    }

    private void ZeroVelocityOnIdle()
    {
        if (movement.Direction == Vector2.zero)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

}





}