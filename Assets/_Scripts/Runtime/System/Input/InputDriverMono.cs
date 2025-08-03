using UnityEngine;
using UnityEngine.InputSystem;


namespace Momentum
{


    public class InputDriverMono : MonoBehaviour, IInputDriverMono
    {
        private InputActions inputAction;

        private void Awake()
        {
            inputAction = new();
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

            inputAction.Player.Dash.started                 += OnDashStarted;

            inputAction.Player.MouseClickLeft.performed     += OnMouseClickLeft;
            inputAction.Player.MouseClickRight.performed    += OnMouseClickRight;

            inputAction.Player.MouseClickLeft.canceled      += OnMouseClickLeftCanceled;
            inputAction.Player.MouseClickRight.canceled     += OnMouseClickRightCanceled;
        }

        private void Unsubscribe()
        {
            inputAction.Player.Dash.started                 -= OnDashStarted;

            inputAction.Player.MouseClickLeft.performed     -= OnMouseClickLeft;
            inputAction.Player.MouseClickRight.performed    -= OnMouseClickRight;

            inputAction.Player.MouseClickLeft.canceled      -= OnMouseClickLeftCanceled;
            inputAction.Player.MouseClickRight.canceled     -= OnMouseClickRightCanceled;

            inputAction.Disable();
        }


        public Vector2 GetMovement()
        {
            return inputAction.Player.Move.ReadValue<Vector2>();
        }

        public Vector2 GetMousePosition()
        {
            return Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : Vector2.zero;
        }

        private void OnDashStarted(InputAction.CallbackContext _ctx)
        {
            EventBus<DashInput>.Raise(new DashInput());
        }

        private void OnMouseClickLeft(InputAction.CallbackContext _ctx)
        {
            EventBus<MouseClickLeft>.Raise(new MouseClickLeft());
        }

        private void OnMouseClickRight(InputAction.CallbackContext _ctx)
        {
            EventBus<MouseClickRight>.Raise(new MouseClickRight());
        }

        private void OnMouseClickLeftCanceled(InputAction.CallbackContext _ctx)
        {
            EventBus<MouseClickLeftCancel>.Raise(new MouseClickLeftCancel());
        }

        private void OnMouseClickRightCanceled(InputAction.CallbackContext _ctx)
        {
            EventBus<MouseClickRightCancel>.Raise(new MouseClickRightCancel());
        }
    }
}