using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI.Components
{
    // Cuts a circular hole in the middle of the graphic by rebuilding its mesh,
    // so it renders with the default UI shader on every platform.
    [RequireComponent(typeof(Graphic))]
    public class IrisHole : BaseMeshEffect
    {
        [SerializeField, Range(0f, 1f)] private float radius;
        [SerializeField, Min(0f)] private float softness = 20f;
        [SerializeField, Range(16, 256)] private int segments = 96;

        private readonly List<UIVertex> sourceVertices = new();
        private Vector2 rectMin;
        private Vector2 rectSize;
        private Vector2 uvMin;
        private Vector2 uvSize;

        public float Radius
        {
            get => radius;
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(radius, value))
                    return;

                radius = value;
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            vh.GetUIVertexStream(sourceVertices);
            if (sourceVertices.Count == 0)
                return;

            Vector2 posMax = Vector2.negativeInfinity;
            Vector2 uvMax = Vector2.negativeInfinity;
            rectMin = Vector2.positiveInfinity;
            uvMin = Vector2.positiveInfinity;
            foreach (UIVertex vertex in sourceVertices)
            {
                rectMin = Vector2.Min(rectMin, vertex.position);
                posMax = Vector2.Max(posMax, vertex.position);
                uvMin = Vector2.Min(uvMin, vertex.uv0);
                uvMax = Vector2.Max(uvMax, vertex.uv0);
            }
            rectSize = posMax - rectMin;
            uvSize = uvMax - uvMin;

            Color32 color = sourceVertices[0].color;
            Color32 transparent = color;
            transparent.a = 0;

            vh.Clear();
            if (radius >= 1f || rectSize.x <= 0f || rectSize.y <= 0f)
                return;

            Vector2 center = rectMin + rectSize * 0.5f;
            float maxRadius = rectSize.magnitude * 0.5f + softness;
            float holeRadius = radius * maxRadius;
            float innerRadius = Mathf.Max(holeRadius - softness, 0f);
            // Polygon must fully contain the rect corners.
            float outerRadius = maxRadius / Mathf.Cos(Mathf.PI / segments);

            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                AddVertex(vh, center + direction * innerRadius, transparent);
                AddVertex(vh, center + direction * holeRadius, color);
                AddVertex(vh, center + direction * outerRadius, color);
            }

            for (int i = 0; i < segments; i++)
            {
                int current = i * 3;
                int next = (i + 1) % segments * 3;

                vh.AddTriangle(current, current + 1, next + 1);
                vh.AddTriangle(current, next + 1, next);
                vh.AddTriangle(current + 1, current + 2, next + 2);
                vh.AddTriangle(current + 1, next + 2, next + 1);
            }
        }

        private void AddVertex(VertexHelper vh, Vector2 position, Color32 color)
        {
            Vector2 normalized = (position - rectMin) / rectSize;
            Vector2 uv = uvMin + Vector2.Scale(normalized, uvSize);
            vh.AddVert(position, color, new Vector4(uv.x, uv.y, 0f, 0f));
        }
    }
}
