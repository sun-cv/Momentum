#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public static class GizmoTools
{
    static float fillAlpha = Settings.Debug.GIZMO_ALPHA;

    public static void DrawCollider(Collider2D collider, bool filled = false)
    {
        switch (collider)
        {
            case BoxCollider2D box:
                DrawBox2D(box, filled);
                break;

            case CircleCollider2D circle:
                DrawCircle2D(circle, filled);
                break;

            case CapsuleCollider2D capsule:
                DrawCapsule2D(capsule, filled);
                break;

            case CompositeCollider2D composite:
                DrawComposite2D(composite, filled);
                break;
        }
    }

    static void DrawBox2D(BoxCollider2D box, bool filled)
    {
        Vector3 center   = box.transform.TransformPoint(box.offset);
        Vector2 halfSize = box.size * 0.5f;

        Vector3[] worldCorners = new Vector3[4];
        Vector2[] localCorners = { new(-halfSize.x, -halfSize.y), new(halfSize.x, -halfSize.y),
                                   new(halfSize.x,  halfSize.y),  new(-halfSize.x, halfSize.y) };

        for (int i = 0; i < 4; i++)
            worldCorners[i] = box.transform.TransformPoint(box.offset + localCorners[i]);

        if (filled)
        {
#if UNITY_EDITOR
            Handles.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, fillAlpha);
            Handles.DrawAAConvexPolygon(worldCorners);
#endif
        }

        Gizmos.DrawLine(worldCorners[0], worldCorners[1]);
        Gizmos.DrawLine(worldCorners[1], worldCorners[2]);
        Gizmos.DrawLine(worldCorners[2], worldCorners[3]);
        Gizmos.DrawLine(worldCorners[3], worldCorners[0]);
    }

    static void DrawCircle2D(CircleCollider2D circle, bool filled)
    {
        Vector3 center = circle.transform.TransformPoint(circle.offset);
        float radius   = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

        if (filled)
        {
#if UNITY_EDITOR
            int segments     = 32;
            Vector3[] verts  = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                verts[i]    = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }
            Handles.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, fillAlpha);
            Handles.DrawAAConvexPolygon(verts);
#endif
        }

        int seg           = 32;
        Vector3 lastPoint = center + new Vector3(radius, 0);
        for (int i = 1; i <= seg; i++)
        {
            float angle      = i / (float)seg * 360f * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint        = newPoint;
        }
    }

    static void DrawCapsule2D(CapsuleCollider2D capsule, bool filled)
    {
        Vector3 center          = capsule.transform.TransformPoint(capsule.offset);
        Vector2 size            = capsule.size * capsule.transform.lossyScale;

        float radius            = Mathf.Min(size.x, size.y) * 0.5f;
        float height            = Mathf.Max(size.x, size.y);
        float halfLength        = (height * 0.5f) - radius;

        bool vertical           = capsule.direction == CapsuleDirection2D.Vertical;
        Vector3 dir             = vertical ? capsule.transform.up    : capsule.transform.right;
        Vector3 perp            = vertical ? capsule.transform.right : capsule.transform.up;

        Vector3 topCenter       = center + dir  * halfLength;
        Vector3 bottomCenter    = center - dir  * halfLength;

        if (filled)
        {
#if UNITY_EDITOR
            int segments        = 16;
            Vector3[] verts     = new Vector3[segments * 2];

            for (int i = 0; i < segments; i++)
            {
                float angle  = i / (float)(segments - 1) * Mathf.PI;
                verts[i]     = topCenter + (perp * Mathf.Cos(angle) + dir * Mathf.Sin(angle)) * radius;
            }

            for (int i = 0; i < segments; i++)
            {
                float angle  = i / (float)(segments - 1) * Mathf.PI;
                verts[segments + i] = bottomCenter + (perp * Mathf.Cos(angle) - dir * Mathf.Sin(angle)) * radius;
            }

            System.Array.Reverse(verts, segments, segments);

            Handles.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, fillAlpha);
            Handles.DrawAAConvexPolygon(verts);
#endif
        }

        int seg          = 32;
        Vector3 lastTop  = topCenter    + perp * radius;
        Vector3 lastBot  = bottomCenter - perp * radius;

        for (int i = 1; i <= seg / 2; i++)
        {
            float angle      = i / (float)(seg / 2) * Mathf.PI;
            Vector3 newTop   = topCenter    + (perp * Mathf.Cos(angle) + dir  * Mathf.Sin(angle)) * radius;
            Vector3 newBot   = bottomCenter + (perp * Mathf.Cos(angle) - dir  * Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(lastTop, newTop);
            Gizmos.DrawLine(lastBot, newBot);

            lastTop = newTop;
            lastBot = newBot;
        }

        Gizmos.DrawLine(topCenter    + perp * radius,  bottomCenter + perp * radius);
        Gizmos.DrawLine(topCenter    - perp * radius,  bottomCenter - perp * radius);
    }

    static void DrawComposite2D(CompositeCollider2D composite, bool filled)
    {
        for (int i = 0; i < composite.pathCount; i++)
        {
            Vector2[] path    = new Vector2[composite.GetPathPointCount(i)];
            composite.GetPath(i, path);

            Vector3[] world   = new Vector3[path.Length];
            for (int j = 0; j < path.Length; j++)
                world[j] = composite.transform.TransformPoint(path[j]);

            if (filled)
            {
#if UNITY_EDITOR
                Handles.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, fillAlpha);
                Handles.DrawAAConvexPolygon(world);
#endif
            }

            for (int j = 0; j < path.Length; j++)
                Gizmos.DrawLine(world[j], world[(j + 1) % path.Length]);
        }
    }
}