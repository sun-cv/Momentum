using UnityEngine;
using character.context;


namespace character.controller
{


public class FacingControl : ActionControl
{
    private Rigidbody2D                 body;
    
    private Context                     context;
    private MovementContext             movement;
    
    private Facing                      currentFacing;

    public FacingControl(Context _context)
    {
        context     = _context;
        movement    = context.Movement;
        body        = context.Core.Body;
    }

    public override void Tick()
    {
        ManageFacing();
    }

    private void ManageFacing()
    {
        if (!FacingChange())
        {
            return;
        }

        SetCurrentFacing();
        ChangeFacing();
    }

    private void SetCurrentFacing()
    {
        currentFacing = context.Movement.Facing;
    }

    private bool FacingChange()
    {
        return currentFacing != context.Movement.Facing;
    }

    private void ChangeFacing()
    {
        Facing f = context.Movement.Facing;

        if (f == Facing.East || f == Facing.NorthEast || f == Facing.SouthEast)
        {
            body.transform.localScale = new Vector3(-1, 1, 1);
        }

        if (f == Facing.West || f == Facing.NorthWest || f == Facing.SouthWest)
        {
            body.transform.localScale = new Vector3(1, 1, 1);
        }

    }

}
}