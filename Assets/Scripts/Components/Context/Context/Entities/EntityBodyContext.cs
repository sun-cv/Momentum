

using UnityEngine;

namespace Momentum
{

    public class EntityBodyContext : ContextRoot
    {
        public Transform transform;
        public Rigidbody2D rigidBody;

        public BoxCollider2D        colliderBox;
        public CapsuleCollider2D    colliderCapsule;
        public CircleCollider2D     colliderCircle;
    }


}