using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Game.Realm;
using Game.Common;



namespace Game.Tooling
{
    public sealed class HitboxGizmos
    {
        private const int Segments = 32;

        private readonly World World;

        private readonly List<Collider2D> colliders = new();
        private readonly List<Vector2>    path      = new();
        private readonly List<Vector3>    points    = new();

        public HitboxGizmos(World world)
        {
            World = world;
        }

        public void Draw()
        {
            if (!Config.Gizmo.Hitboxes)
                return;

            foreach (var hitbox in World.Query(Mask<Components, Hitbox, Form>.Key))
            {
                DrawBody(World.Entity.Form(hitbox).Body, Config.Gizmo.Hitbox);
            }
        }

        // Body
        private void DrawBody(Rigidbody2D body, Color color)
        {
            var pose = Matrix4x4.TRS(body.position, Quaternion.Euler(0f, 0f, body.rotation), body.transform.lossyScale);

            body.GetAttachedColliders(colliders);

            foreach (var collider in colliders)
            {
                var local = body.transform.worldToLocalMatrix * collider.transform.localToWorldMatrix;

                DrawCollider(collider, pose * local, color);
            }
        }

        private void DrawCollider(Collider2D collider, Matrix4x4 matrix, Color color)
        {
            if (collider.compositeOperation != Collider2D.CompositeOperation.None)
                return;

            switch (collider)
            {
                case CompositeCollider2D composite:
                    Composite(composite, matrix, color);
                    break;

                case BoxCollider2D box:
                    Box(box);
                    Draw(matrix, color, true);
                    break;

                case CircleCollider2D circle:
                    Circle(circle);
                    Draw(matrix, color, true);
                    break;

                case CapsuleCollider2D capsule:
                    Capsule(capsule);
                    Draw(matrix, color, true);
                    break;

                case PolygonCollider2D polygon:
                    Polygon(polygon, matrix, color);
                    break;
            }
        }

        // Shapes
        private void Composite(CompositeCollider2D composite, Matrix4x4 matrix, Color color)
        {
            for (var i = 0; i < composite.pathCount; i++)
            {
                composite.GetPath(i, path);

                points.Clear();

                foreach (var point in path)
                {
                    points.Add(composite.offset + point);
                }

                Draw(matrix, color, false);
            }
        }

        private void Box(BoxCollider2D box)
        {
            var half = box.size * .5f;

            points.Clear();
            points.Add(box.offset + new Vector2(-half.x, -half.y));
            points.Add(box.offset + new Vector2( half.x, -half.y));
            points.Add(box.offset + new Vector2( half.x,  half.y));
            points.Add(box.offset + new Vector2(-half.x,  half.y));
        }

        private void Circle(CircleCollider2D circle)
        {
            points.Clear();

            for (var i = 0; i < Segments; i++)
            {
                var angle = i * Mathf.PI * 2f / Segments;

                points.Add(circle.offset + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * circle.radius);
            }
        }

        private void Capsule(CapsuleCollider2D capsule)
        {
            var vertical = capsule.direction == CapsuleDirection2D.Vertical;
            var radius   = Mathf.Min(capsule.size.x, capsule.size.y) * .5f;
            var length   = Mathf.Max(capsule.size.x, capsule.size.y) * .5f - radius;
            var axis     = vertical ? Vector2.up    : Vector2.right;
            var side     = vertical ? Vector2.right : Vector2.up;

            points.Clear();

            Arc(capsule.offset + axis * length,  side,  axis, radius);
            Arc(capsule.offset - axis * length, -side, -axis, radius);
        }

        private void Arc(Vector2 center, Vector2 from, Vector2 toward, float radius)
        {
            var half = Segments / 2;

            for (var i = 0; i <= half; i++)
            {
                var angle = i * Mathf.PI / half;

                points.Add(center + (from * Mathf.Cos(angle) + toward * Mathf.Sin(angle)) * radius);
            }
        }

        private void Polygon(PolygonCollider2D polygon, Matrix4x4 matrix, Color color)
        {
            for (var i = 0; i < polygon.pathCount; i++)
            {
                polygon.GetPath(i, path);

                points.Clear();

                foreach (var point in path)
                {
                    points.Add(polygon.offset + point);
                }

                Draw(matrix, color, false);
            }
        }

        // Draw
        private void Draw(Matrix4x4 matrix, Color color, bool filled)
        {
            Gizmos.matrix = matrix;
            Gizmos.color  = color;

            for (var i = 0; i < points.Count; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
            }

            Gizmos.matrix = Matrix4x4.identity;

            if (filled)
                Fill(matrix, color);
        }

        private void Fill(Matrix4x4 matrix, Color color)
        {
#if UNITY_EDITOR
            Handles.matrix = matrix;
            Handles.color  = new Color(color.r, color.g, color.b, Config.Gizmo.Fill);

            Handles.DrawAAConvexPolygon(points.ToArray());

            Handles.matrix = Matrix4x4.identity;
#endif
        }
    }
}
