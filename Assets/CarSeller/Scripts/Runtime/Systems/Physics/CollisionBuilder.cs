using UnityEngine;

public static class CollisionBuilder
{
    #region collision
    public static void InitializeCollision(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return;

        var collider = sr.gameObject.GetComponent<Collider2D>();

        // 1) If collider exists: adjust based on type.
        if (collider != null)
        {
            switch (collider)
            {
                case PolygonCollider2D poly:
                    RegeneratePolygon(poly, sr);
                    return;

                case BoxCollider2D box:
                    AdjustBox(box, sr);
                    return;

                case CircleCollider2D circle:
                    AdjustCircle(circle, sr);
                    return;

                // Other collider types are ignored intentionally.
                default:
                    return;
            }
        }

        // 2) If none exists: add PolygonCollider2D and regenerate.
        var newPoly = sr.gameObject.AddComponent<PolygonCollider2D>();
        RegeneratePolygon(newPoly, sr);
    }

    private static void RegeneratePolygon(PolygonCollider2D poly, SpriteRenderer sr)
    {
        if (poly == null || sr == null || sr.sprite == null) return;

        var sprite = sr.sprite;
        int shapeCount = sprite.GetPhysicsShapeCount();

        if (shapeCount == 0)
        {
            // Fallback: use sprite rectangular bounds.
            var b = sprite.bounds;
            Vector2 min = b.min;
            Vector2 max = b.max;
            poly.pathCount = 1;
            poly.SetPath(0, new[]
            {
                new Vector2(min.x, min.y),
                new Vector2(min.x, max.y),
                new Vector2(max.x, max.y),
                new Vector2(max.x, min.y)
            });
            return;
        }

        poly.pathCount = shapeCount;
        var points = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i < shapeCount; i++)
        {
            points.Clear();
            sprite.GetPhysicsShape(i, points);
            // Points are already in local sprite space; assign directly.
            poly.SetPath(i, points);
        }
    }

    private static void AdjustBox(BoxCollider2D box, SpriteRenderer sr)
    {
        if (box == null || sr == null || sr.sprite == null) return;

        // Use sprite local bounds for size and offset.
        var b = sr.sprite.bounds;
        box.size = b.size;
        box.offset = b.center;
    }

    private static void AdjustCircle(CircleCollider2D circle, SpriteRenderer sr)
    {
        if (circle == null || sr == null || sr.sprite == null) return;

        var b = sr.sprite.bounds;
        // Radius = half of max dimension.
        float radius = Mathf.Max(b.size.x, b.size.y) * 0.5f;
        circle.radius = radius;
        circle.offset = b.center;
    }
    #endregion
}