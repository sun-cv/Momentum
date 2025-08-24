namespace Momentum
{


    public class Hero : Entity
    {
        
        private readonly IInput      input;
        private readonly IMovement   movement;

        private readonly MovementIntent movementIntent = new();


        public void OnAwake()
        {
            GetComponent<IInput>();

            input   .BindMovementIntent(movementIntent);
            movement.BindMovementIntent(movementIntent);

    

        }

        public void Update()
        {



        }


    }
}
