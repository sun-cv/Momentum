using UnityEngine;
using UnityEngine.InputSystem;


namespace Momentum
{

    public class InputDriverMono : MonoBehaviour
    {
        private InputActions inputAction;

        void Awake()
        {
            inputAction = new();
        }

        void OnEnable()
        {
            inputAction.Player.Enable();
            Subscribe();
        }

        void OnDisable()
        {
            inputAction.Player.Disable();
            Unsubscribe();
        }

        void Subscribe()
        {
            inputAction.Enable();

            inputAction.Player.Interact.performed           += OnInteractPress;
            inputAction.Player.Interact.canceled            += OnInteractRelease;

            inputAction.Player.Attack.performed             += OnAttackPress;
            inputAction.Player.Attack.canceled              += OnAttackRelease;

            inputAction.Player.Block.performed              += OnBlockPress;
            inputAction.Player.Block.canceled               += OnBlockRelease;

            inputAction.Player.Dash.performed               += OnDashPress;
            inputAction.Player.Dash.canceled                += OnDashRelease;

            inputAction.Player.Aim.performed                += OnAimPress;
            inputAction.Player.Aim.canceled                 += OnAimRelease;
        }

        void Unsubscribe()
        {

            inputAction.Player.Interact.performed           -= OnInteractPress;
            inputAction.Player.Interact.canceled            -= OnInteractRelease;

            inputAction.Player.Attack.performed             -= OnAttackPress;
            inputAction.Player.Attack.canceled              -= OnAttackRelease;

            inputAction.Player.Block.performed              -= OnBlockPress;
            inputAction.Player.Block.canceled               -= OnBlockRelease;

            inputAction.Player.Dash.performed               -= OnDashPress;
            inputAction.Player.Dash.canceled                -= OnDashRelease;

            inputAction.Player.Aim.performed                -= OnAimPress;
            inputAction.Player.Aim.canceled                 -= OnAimRelease;

            inputAction.Disable();
        }

        void Update()
        {
            EventBus<MousePosition> .Raise(new(Mouse.current.position?.ReadValue() ?? Vector2.zero));
            EventBus<MovementVector>.Raise(new(inputAction.Player.Move.ReadValue<Vector2>()));
        }

        void OnInteractPress(InputAction.CallbackContext _ctx)      => EventBus<InteractPress>  .Raise(new ());
        void OnInteractRelease(InputAction.CallbackContext _ctx)    => EventBus<InteractRelease>.Raise(new ());

        void OnAttackPress(InputAction.CallbackContext _ctx)        => EventBus<AttackPress>    .Raise(new ());
        void OnAttackRelease(InputAction.CallbackContext _ctx)      => EventBus<AttackRelease>  .Raise(new ());

        void OnBlockPress(InputAction.CallbackContext _ctx)         => EventBus<BlockPress>     .Raise(new ());
        void OnBlockRelease(InputAction.CallbackContext _ctx)       => EventBus<BlockRelease>   .Raise(new ());

        void OnDashPress(InputAction.CallbackContext _ctx)          => EventBus<DashPress>      .Raise(new ());
        void OnDashRelease(InputAction.CallbackContext _ctx)        => EventBus<DashRelease>    .Raise(new ());
        
        void OnAimPress(InputAction.CallbackContext _ctx)           => EventBus<AimPress>       .Raise(new ());
        void OnAimRelease(InputAction.CallbackContext _ctx)         => EventBus<AimRelease>     .Raise(new ());
    
        public Vector2 GetMovement()        => Mouse.current.position?.ReadValue() ?? Vector2.zero;
        public Vector2 GetMousePosition()   => inputAction.Player.Move.ReadValue<Vector2>();
    }
}