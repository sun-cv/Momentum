using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Game.Realm;
using Game.Common;

using Style = Game.Common.Config.Gizmo.Style;



namespace Game.Tooling
{
    public sealed class HitboxGizmos
    {
        private const int Segments = 32;

        private readonly World World;

        private readonly Material         material;
        private readonly List<Collider2D> colliders = new();
        private readonly List<Vector2>    path      = new();
        private readonly List<Vector3>    points    = new();
        private readonly List<int>        remaining = new();

        public HitboxGizmos(World world)
        {
            World    = world;
            material = new Material(Shader.Find("Hidden/Internal-Colored")) { hideFlags = HideFlags.HideAndDontSave };

            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_Cull",     (int)CullMode.Off);
            material.SetInt("_ZWrite",   0);
        }

        public void Draw()
        {
            if (!Config.Gizmo.Hitboxes)
                return;

            foreach (var hitbox in World.Query(Mask<Components, Hitbox, Form>.Key))
            {
                DrawBody(World.Entity.Form(hitbox).Body, Config.Gizmo.Hitbox);
            }

            foreach (var entity in World.Query(Mask<Components, Hurtbox, Form>.Key))
            {
                DrawAttached(World.Entity.Form(entity).Body, World.Entity.Hurtbox(entity).Collider, Config.Gizmo.Hurtbox);
            }

            foreach (var entity in World.Query(Mask<Components, Bodybox, Form>.Key))
            {
                DrawAttached(World.Entity.Form(entity).Body, World.Entity.Bodybox(entity).Collider, Config.Gizmo.Bodybox);
            }
        }

        // Body
        private void DrawBody(Rigidbody2D body, Style style)
        {
            body.GetAttachedColliders(colliders);

            foreach (var collider in colliders)
            {
                DrawAttached(body, collider, style);
            }
        }

        private void DrawAttached(Rigidbody2D body, Collider2D collider, Style style)
        {
            var pose  = Matrix4x4.TRS(body.position, Quaternion.Euler(0f, 0f, body.rotation), body.transform.lossyScale);
            var local = body.transform.worldToLocalMatrix * collider.transform.localToWorldMatrix;

            DrawCollider(collider, pose * local, style);
        }

        private void DrawCollider(Collider2D collider, Matrix4x4 matrix, Style style)
        {
            if (collider.compositeOperation != Collider2D.CompositeOperation.None)
                return;

            switch (collider)
            {
                case CompositeCollider2D composite:
                    Composite(composite, matrix, style);
                    break;

                case BoxCollider2D box:
                    Box(box);
                    Draw(matrix, style);
                    break;

                case CircleCollider2D circle:
                    Circle(circle);
                    Draw(matrix, style);
                    break;

                case CapsuleCollider2D capsule:
                    Capsule(capsule);
                    Draw(matrix, style);
                    break;

                case PolygonCollider2D polygon:
                    Polygon(polygon, matrix, style);
                    break;
            }
        }

        // Shapes
        private void Box(BoxCollider2D box)
        {
            var half   = box.size * .5f;
            var radius = box.edgeRadius;

            points.Clear();

            if (radius <= 0f)
            {
                points.Add(box.offset + new Vector2(-half.x, -half.y));
                points.Add(box.offset + new Vector2( half.x, -half.y));
                points.Add(box.offset + new Vector2( half.x,  half.y));
                points.Add(box.offset + new Vector2(-half.x,  half.y));
                return;
            }

            Corner(box.offset + new Vector2( half.x,  half.y), radius,   0f);
            Corner(box.offset + new Vector2(-half.x,  half.y), radius,  90f);
            Corner(box.offset + new Vector2(-half.x, -half.y), radius, 180f);
            Corner(box.offset + new Vector2( half.x, -half.y), radius, 270f);
        }

        private void Corner(Vector2 center, float radius, float start)
        {
            var quarter = Segments / 4;

            for (var i = 0; i <= quarter; i++)
            {
                var angle = (start + i * 90f / quarter) * Mathf.Deg2Rad;

                points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
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

        private void Polygon(PolygonCollider2D polygon, Matrix4x4 matrix, Style style)
        {
            for (var i = 0; i < polygon.pathCount; i++)
            {
                polygon.GetPath(i, path);

                points.Clear();

                foreach (var point in path)
                {
                    points.Add(polygon.offset + point);
                }

                Draw(matrix, style);
            }
        }

        private void Composite(CompositeCollider2D composite, Matrix4x4 matrix, Style style)
        {
            for (var i = 0; i < composite.pathCount; i++)
            {
                composite.GetPath(i, path);

                points.Clear();

                foreach (var point in path)
                {
                    points.Add(composite.offset + point);
                }

                Draw(matrix, style);
            }
        }

        // Draw
        private void Draw(Matrix4x4 matrix, Style style)
        {
            Fill(matrix, style.Fill);
            Outline(matrix, style.Line);
        }

        private void Outline(Matrix4x4 matrix, Color color)
        {
            Gizmos.matrix = matrix;
            Gizmos.color  = color;

            for (var i = 0; i < points.Count; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private void Fill(Matrix4x4 matrix, Color color)
        {
            material.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(matrix);
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            Triangulate();

            GL.End();
            GL.PopMatrix();
        }

        // Triangulation
        private void Triangulate()
        {
            var winding = Area() > 0f ? 1f : -1f;

            remaining.Clear();

            for (var i = 0; i < points.Count; i++)
            {
                remaining.Add(i);
            }

            while (remaining.Count > 3)
            {
                if (!ClipEar(winding))
                    return;
            }

            Triangle(points[remaining[0]], points[remaining[1]], points[remaining[2]]);
        }

        private bool ClipEar(float winding)
        {
            for (var i = 0; i < remaining.Count; i++)
            {
                var previous = remaining[(i + remaining.Count - 1) % remaining.Count];
                var current  = remaining[i];
                var next     = remaining[(i + 1) % remaining.Count];

                if (!IsEar(previous, current, next, winding))
                    continue;

                Triangle(points[previous], points[current], points[next]);
                remaining.RemoveAt(i);

                return true;
            }

            return false;
        }

        private static void Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            GL.Vertex(a);
            GL.Vertex(b);
            GL.Vertex(c);
        }

        // Geometry
        private bool IsEar(int previous, int current, int next, float winding)
        {
            var a = points[previous];
            var b = points[current];
            var c = points[next];

            if (Cross(a, b, c) * winding <= 0f)
                return false;

            foreach (var index in remaining)
            {
                if (index == previous || index == current || index == next)
                    continue;

                if (Inside(points[index], a, b, c, winding))
                    return false;
            }

            return true;
        }

        private float Area()
        {
            var sum = 0f;

            for (var i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];

                sum += a.x * b.y - b.x * a.y;
            }

            return sum;
        }

        private static float Cross(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        private static bool Inside(Vector3 point, Vector3 a, Vector3 b, Vector3 c, float winding)
        {
            return Cross(a, b, point) * winding >= 0f
                && Cross(b, c, point) * winding >= 0f
                && Cross(c, a, point) * winding >= 0f;
        }
    }
}
