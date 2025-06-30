using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterMovement : MonoBehaviour
{   
    private Rigidbody2D                 body;
    private CharacterContext            context;
    private CharacterContextMovement    movement;


    [Header("Movement Configuration")]
    [SerializeField] private float      acceleration;
    [SerializeField] private float      maxSpeed;

    [Header("Movement Force Adjustment")]
    [SerializeField] private bool       velocityClamp;
    [SerializeField] private bool       velocityZero;

    public void Initialize(CharacterContext _context)
    {
        context         = _context;
        movement        = _context.Movement;
        body            = _context.Core.Body;
    }

    public void Tick()
    {

    }

    public void TickFixed()
    {
        MovementForce();
    }


    private void MovementForce()
    {
        ApplyMovementForce();
        ClampVelocityToMax();
        ZeroVelocityOnIdle();
    }

    private void ApplyMovementForce()
    {
        if (body.linearVelocity.magnitude < 0.1f && movement.Direction != Vector2.zero)
        {
            body.linearVelocity = movement.Direction * acceleration;
        }
        else
        {
            body.AddForce(movement.Direction * acceleration, ForceMode2D.Impulse);
        }
    }

    private void ClampVelocityToMax()
    {
        if (body.linearVelocity.magnitude > maxSpeed)
        {
            body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
        }
    }

    private void ZeroVelocityOnIdle()
    {
        if (movement.Direction == Vector2.zero)
        {
            body.linearVelocity = Vector2.zero;
        }
    }


}
