using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class HitboxController : Controller
{
    public Guid HitboxId            { get; set; }
    public HitboxManager Manager    { get; set; }
    public Actor Owner              { get; set; }
    
    readonly Color gizmoColor        = Color.red;

    private void OnTriggerEnter(Collider trigger)
    {
        if (!trigger.TryGetComponent<BridgeController>(out var target))
            return;

        Manager.DetectHit(new(){ HitboxId = HitboxId, Owner = Owner, Target = target.Bridge.Owner });
    }

    public void Configure(Guid Id, Actor owner, HitboxManager manager)
    {
        if (Owner != null)
            return;

        HitboxId    = Id;
        Owner       = owner;
        Manager     = manager;
    }
    
    private void OnDrawGizmos()
    {

        if (!HitboxManager.ShowDebugGizmos) return;
        
        Gizmos.color = gizmoColor;
        DrawColliders();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        DrawColliders();
    }
    
    void DrawColliders()
    {
        var colliders = GetComponentsInChildren<Collider2D>();

        foreach (var collider in colliders)
            DrawCollider(collider);
    }
    
    void DrawCollider(Collider2D collider)
    {
        switch (collider)
        {
            case BoxCollider2D box:
                DrawBox2D(box);
                break;

            case CircleCollider2D circle:
                DrawCircle2D(circle);
                break;

            case CapsuleCollider2D capsule:
                DrawCapsule2D(capsule);
                break;

            case CompositeCollider2D composite:
                DrawComposite2D(composite);
                break;
        }
    }

    void DrawBox2D(BoxCollider2D box)
    {
        Vector3 center      = box.transform.TransformPoint(box.offset);
        Vector2 size        = box.size;
        Vector2 halfSize    = size * 0.5f;

        Vector2[] localCorners = new Vector2[4]
        {
            new(-halfSize.x, -halfSize.y),
            new( halfSize.x, -halfSize.y),
            new( halfSize.x,  halfSize.y),
            new(-halfSize.x,  halfSize.y)
        };

        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = box.transform.TransformPoint(box.offset + localCorners[i]);
        }

        Gizmos.DrawLine(worldCorners[0], worldCorners[1]);
        Gizmos.DrawLine(worldCorners[1], worldCorners[2]);
        Gizmos.DrawLine(worldCorners[2], worldCorners[3]);
        Gizmos.DrawLine(worldCorners[3], worldCorners[0]);
    }

    void DrawCircle2D(CircleCollider2D circle)
    {
        Vector3 center      = circle.transform.TransformPoint(circle.offset);
        float radius        = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

        int segments        = 32;
        Vector3 lastPoint   = center + new Vector3(radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i / (float)segments * 360f * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint   = newPoint;
        }
    }

    void DrawCapsule2D(CapsuleCollider2D capsule)
    {
        Vector3 center  = capsule.transform.TransformPoint(capsule.offset);
        Vector2 size    = capsule.size * capsule.transform.lossyScale;

        float radius    = Mathf.Min(size.x, size.y) * 0.5f;
        float height    = Mathf.Max(size.x, size.y);

        Vector3 direction;
        
        if (capsule.direction == CapsuleDirection2D.Vertical)
            direction = capsule.transform.up;
        else
            direction = capsule.transform.right;

        float halfLength = (height * 0.5f - radius);

        Gizmos.DrawWireSphere(center + direction * halfLength, radius);
        Gizmos.DrawWireSphere(center - direction * halfLength, radius);

        Vector3 perpendicular = capsule.direction == CapsuleDirection2D.Vertical 
            ? capsule.transform.right 
            : capsule.transform.up;

        Vector3 offset = perpendicular * radius;
        Gizmos.DrawLine(center + direction * halfLength + offset, center - direction * halfLength + offset);
        Gizmos.DrawLine(center + direction * halfLength - offset, center - direction * halfLength - offset);
    }

    void DrawComposite2D(CompositeCollider2D composite)
    {
        for (int i = 0; i < composite.pathCount; i++)
        {
            Vector2[] path = new Vector2[composite.GetPathPointCount(i)];
            composite.GetPath(i, path);

            for (int j = 0; j < path.Length; j++)
            {
                Vector3 start   = composite.transform.TransformPoint(path[j]);
                Vector3 end     = composite.transform.TransformPoint(path[(j + 1) % path.Length]);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
