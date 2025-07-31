using UnityEngine;


namespace Momentum.Actor.Sensor
{


public class RaycastSensor
{
    public enum CastDirection { Up, Down, Left, Right }

    Transform       transform;
    RaycastHit      hitInfo;    
    CastDirection   castDirection;

    Vector3 origin              = Vector3.zero;

    public float castLength     = 1f;
    public LayerMask layerMask  = 255;


    public RaycastSensor(Transform playerTransform)
    {
        transform = playerTransform;
    }

    public void Cast()
    {
        Vector3 worldOrigin     = transform.TransformPoint(origin);
        Vector3 worldDirection  = GetCastDirection();
    
        Physics.Raycast(worldOrigin, worldDirection, out hitInfo, castLength, layerMask, QueryTriggerInteraction.Ignore);
    }

    public bool HasDetectedHit()    => hitInfo.collider != null;
    public float GetDistance()      => hitInfo.distance;
    public Vector3 GetNormal()      => hitInfo.normal;
    public Vector3 GetPosition()    => hitInfo.point;
    public Collider GetCollider()   => hitInfo.collider;
    public Transform GetTransform() => hitInfo.transform;

    public void SetCastDirection(CastDirection direction)   => castDirection = direction;
    public void SetCastOrigin(Vector3 position)             => origin = transform.InverseTransformPoint(position);

    Vector3 GetCastDirection()
    {
        return castDirection switch
        {

            CastDirection.Up    =>  transform.up,
            CastDirection.Down  => -transform.up,
            CastDirection.Left  => -transform.right,
            CastDirection.Right =>  transform.right,
            _                   =>  Vector3.one
        };
    }

}


}