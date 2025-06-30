using UnityEngine;


public class CharacterContextMovement
{
    public Facing       Facing                  { get; private set; }
    public Vector2      Direction               { get; private set; }

    public ManagedFlag  IntentMove              { get; private set; }

        // Requests
    public RequestFlag  RequestDash             { get; private set; }

        // Context
    public bool         IsDashing               { get; private set; }


    public CharacterContextMovement()
    {
        Subscribe();
        GenerateManagedFlags();
        GenerateRequestFlags();
    }


    public void GenerateManagedFlags()
    {
        IntentMove = new ManagedFlag(() => Direction != Vector2.zero);
    }

    public void GenerateRequestFlags()
    {
        RequestDash = new RequestFlag();
    }

        // Subscribers
    public void Subscribe()
    {
        InputHandler.Instance.OnMoveFacing      += SetFacing;
        InputHandler.Instance.OnMoveDirection   += SetDirection;
        InputHandler.Instance.OnDash            += () => RequestDash.Request();
    }
    public void Deconstruct()
    {
        InputHandler.Instance.OnMoveFacing      -= SetFacing;
        InputHandler.Instance.OnMoveDirection   -= SetDirection;
    }

    public void SetDirection(Vector2 _vector)
    {
        Direction = _vector;
        IntentMove.Update();
    }

    public void SetFacing(Facing _facing)
    {
        Facing = _facing;
    }
}