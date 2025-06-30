using UnityEngine;

public class CharacterContextCore
{
    public Rigidbody2D  Body        { get; }
    public Collider2D   Hitbox      { get; }
    public Transform    Transform   { get; }

    public CharacterContextCore(Transform transform, Rigidbody2D rigidBody, Collider2D collider)
    {
        Body        = rigidBody;
        Hitbox      = collider;
        Transform   = transform;
    }

    public static CharacterContextCore From(Transform transform, Rigidbody2D rigidBody, Collider2D collider)
    {
        return new CharacterContextCore(transform, rigidBody, collider);
    }

    public static CharacterContextCore From(Character characterObject)
    {
        return new CharacterContextCore(
            characterObject.transform,
            characterObject.GetComponent<Rigidbody2D>(),
            characterObject.GetComponent<Collider2D>()
        );
    }


}
