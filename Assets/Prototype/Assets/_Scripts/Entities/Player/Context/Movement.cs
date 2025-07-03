using UnityEngine;

namespace character.context
{

public class MovementContext
{
    public Facing           Facing          { get; private set; }
    public Vector2          Direction       { get; private set; }

    public FlagManaged      Intent          { get; private set; }

    public DashContext      Dash                   { get; private set; } = new DashContext();
    public IdleContext      Idle                   { get; private set; } = new IdleContext();
    public SprintContext    Sprint                 { get; private set; } = new SprintContext();


    public MovementContext()
    {
        Subscribe();
        GenerateFlags();
    }

    public void GenerateFlags()
    {
        Intent = new FlagManaged(() => Direction != Vector2.zero);
    }

        // Subscribers
    public void Subscribe()
    {
        InputHandler.Instance.OnMoveFacing      += SetFacing;
        InputHandler.Instance.OnMoveDirection   += SetDirection;
    }
    public void Deconstruct()
    {
        InputHandler.Instance.OnMoveFacing      -= SetFacing;
        InputHandler.Instance.OnMoveDirection   -= SetDirection;

    }

    public void SetDirection(Vector2 _vector)
    {
        Direction = _vector;
        Intent.Update();
    }

    public void SetFacing(Facing _facing)
    {
        Facing = _facing;
    }
}
}