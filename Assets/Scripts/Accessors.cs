using UnityEngine;





public class RemoteVector2
{
    Vector2 value;

    public Vector2 Value
    {
        get => value;
        set => this.value = value;
    }

    public static implicit operator Vector2(RemoteVector2 remote) => remote.value;
}
