using UnityEngine;

public class WorldPosition : RegisteredService, IBind
{
    public Camera Camera                { get; internal set; }
    public InputRouter InputRouter      { get; internal set; }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public Vector2 Mouse()
    {
        return Camera.ScreenToWorldPoint(InputRouter.MousePosition);
    }

    public Vector2 DirectionTo(Vector2 to, Vector2 from)
    {
        return (to - from).normalized;
    }

    public Vector2 MouseDirectionTo(Vector2 position)
    {
        return DirectionTo(position, Mouse());
    }

    public Vector2 MouseDirectionFrom(Vector2 position)
    {
        return DirectionTo(Mouse(), position);
    }
    
    public Intercardinal MouseIntercardinalFrom(Vector2 position)
    {
        return Orientation.FromVector(MouseDirectionFrom(position));
    }

    public float DistanceToMouse(Vector2 position)
    {
        return Vector2.Distance(position, Mouse());
    }

    // ===============================================================================

                    
    public void Bind()
    {
        Camera          = Services.Get<CameraRig>().Camera;
        InputRouter     = Services.Get<InputRouter>();
    }

    public override void Dispose()
    {
        Camera      = null;
        InputRouter = null;

        Services.Deregister(this);
    }
}

