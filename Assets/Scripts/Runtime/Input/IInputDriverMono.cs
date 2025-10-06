using UnityEngine;


namespace Momentum
{
    public interface IInputDriverMono
    {
        public Vector2 GetMovement();
        public Vector2 GetMousePosition();
    }
}