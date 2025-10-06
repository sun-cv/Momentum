using Unity.VisualScripting;
using UnityEngine;

namespace Momentum
{

    public interface IMovementAttribute : IRuntimeAttribute
    {
        public float MovementSpeed { get; }
    }


    [CreateAssetMenu(menuName = "Momentum/Entity/Attribute/Movement")]
    public class MovementAttribute : Attribute
    {

        public float movementSpeed;
    
        public override IRuntimeAttribute CreateRuntime() => new Runtime(this);

        sealed class Runtime : Stats, IMovementAttribute
        {
            MovementAttribute attribute;

            public Runtime(MovementAttribute instance) => attribute = instance;

            public float MovementSpeed => Resolve(nameof(MovementSpeed), attribute.movementSpeed);
        }
    }
}

