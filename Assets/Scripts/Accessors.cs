using UnityEngine;





public class RemoteVector2
{
    Vector2 vector;

    public Vector2 Vector
    {
        get => vector;
        set => this.vector = value;
    }

    public static implicit operator Vector2(RemoteVector2 remote) => remote.vector;
}
