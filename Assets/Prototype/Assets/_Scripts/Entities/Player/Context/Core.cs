using UnityEngine;


namespace character.context
{


public class ContextCore
{
    public Rigidbody2D  Body        { get; }
    public Collider2D   Hitbox      { get; }
    public Transform    Transform   { get; }

    public ContextCore(Transform transform, Rigidbody2D rigidBody, Collider2D collider)
    {
        Body        = rigidBody;
        Hitbox      = collider;
        Transform   = transform;
    }

    public static ContextCore From(Transform transform, Rigidbody2D rigidBody, Collider2D collider)
    {
        return new ContextCore(transform, rigidBody, collider);
    }

    public static ContextCore From(Character characterObject)
    {
        return new ContextCore(
            characterObject.transform,
            characterObject.GetComponent<Rigidbody2D>(),
            characterObject.GetComponent<Collider2D>()
        );
    }
}
}