using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    private InputActions inputAction;

    public event Action<Vector2>    OnMouseMovePosition;
    public event Action             OnMouseClickLeft;
    public event Action             OnMouseClickRight;
    public event Action             OnMouseClickLeftCancel;
    public event Action             OnMouseClickRightCancel;


    public event Action<Facing>     OnMoveFacing;
    public event Action<Vector2>    OnMoveDirection;

    public event Action             OnDash;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance    = this;
        inputAction = new InputActions();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        inputAction.Player.Enable();
        Subscribe();
    }

    private void OnDisable()
    {
        inputAction.Player.Disable();
        Unsubscribe();
    }

    private void Subscribe()
    {
        inputAction.Enable();

        inputAction.Player.Move.performed               += OnMovePerformed;
        inputAction.Player.Move.canceled                += OnMoveCanceled;

        inputAction.Player.Dash.started                 += OnDashStarted;

        inputAction.Player.MouseLocation.performed      += OnMouseMovePerformed;

        inputAction.Player.MouseClickLeft.performed     += OnMouseClickLeftPerformed;
        inputAction.Player.MouseClickLeft.canceled      += OnMouseClickLeftCanceled;

        inputAction.Player.MouseClickRight.performed    += OnMouseClickRightPerformed;
        inputAction.Player.MouseClickRight.canceled     += OnMouseClickRightCanceled;
    }

    private void Unsubscribe()
    {
        inputAction.Player.Move.performed               -= OnMovePerformed;
        inputAction.Player.Move.canceled                -= OnMoveCanceled;

        inputAction.Player.Dash.started                 -= OnDashStarted;

        inputAction.Player.MouseLocation.performed      -= OnMouseMovePerformed;

        inputAction.Player.MouseClickLeft.performed     -= OnMouseClickLeftPerformed;
        inputAction.Player.MouseClickLeft.canceled      -= OnMouseClickLeftCanceled;

        inputAction.Player.MouseClickRight.performed    -= OnMouseClickRightPerformed;
        inputAction.Player.MouseClickRight.canceled     -= OnMouseClickRightCanceled;

        inputAction.Disable();
    }


    private void OnMovePerformed(InputAction.CallbackContext _ctx)
    {
        OnMoveFacing?.Invoke(GetFacing(_ctx.ReadValue<Vector2>()));
        OnMoveDirection?.Invoke(_ctx.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext _ctx)
    {
        OnMoveDirection?.Invoke(Vector2.zero);
    }

    private void OnDashStarted(InputAction.CallbackContext _ctx)
    {
        OnDash?.Invoke();
    }

    private void OnMouseMovePerformed(InputAction.CallbackContext _ctx)
    {
        OnMouseMovePosition?.Invoke(_ctx.ReadValue<Vector2>());
    }

    private void OnMouseClickLeftPerformed(InputAction.CallbackContext _ctx)
    {
        OnMouseClickLeft?.Invoke();
    }

    private void OnMouseClickLeftCanceled(InputAction.CallbackContext _ctx)
    {
        OnMouseClickLeftCancel?.Invoke();
    }

    private void OnMouseClickRightPerformed(InputAction.CallbackContext _ctx)
    {
        OnMouseClickRight?.Invoke();
    }

    private void OnMouseClickRightCanceled(InputAction.CallbackContext _ctx)
    {
        OnMouseClickRightCancel?.Invoke();
    }

    private Facing GetFacing(Vector2 _vector)
    {
        float angle = Mathf.Atan2(_vector.y, _vector.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        float index = Mathf.RoundToInt(angle / 45f);
        
        return (Facing)index;
    }

}


// Add CTRL to cancel?