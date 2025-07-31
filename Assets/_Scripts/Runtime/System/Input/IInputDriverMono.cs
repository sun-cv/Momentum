using UnityEngine;


namespace Momentum.Interface
{
    public interface IInputDriverMono
    {
        public Vector2 GetMovement();
        public Vector2 GetMousePosition();
    }
}