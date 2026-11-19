using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFoundations
{
    /// <summary>
    /// Colors a uGUI mesh with a <see cref="GradientSampler"/>. When no gradient key falls inside the mesh it recolors
    /// the vertices in place; otherwise it slices only the triangles a key crosses, so each key's color shows exactly.
    /// One instance is shared by every group: uGUI rebuilds meshes on the main thread, one at a time.
    /// </summary>
    internal sealed class GradientMeshPainter
    {
        private const int TRIANGLE_VERTEX_COUNT = 3;

        private readonly List<UIVertex> stream = new();
        private readonly List<float> streamTimes = new();
        private readonly List<UIVertex> output = new();
        private readonly List<TimedVertex> polygon = new();
        private readonly List<TimedVertex> below = new();
        private readonly List<TimedVertex> above = new();
        private GradientSampler sampler;
        private Matrix4x4 meshToGroup;

        public void Paint(VertexHelper helper, GradientSampler gradientSampler, Matrix4x4 meshToGroupMatrix)
        {
            sampler = gradientSampler;
            meshToGroup = meshToGroupMatrix;

            if (HasStopInsideMesh(helper))
                PaintSliced(helper);
            else
                PaintInPlace(helper);

            sampler = null;
        }

        private bool HasStopInsideMesh(VertexHelper helper)
        {
            if (sampler.StopCount == 0)
                return false;

            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            UIVertex vertex = new();
            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                float time = CalculateTime(vertex);
                minimum = Mathf.Min(minimum, time);
                maximum = Mathf.Max(maximum, time);
            }
            return sampler.HasStopBetween(minimum, maximum);
        }

        private void PaintInPlace(VertexHelper helper)
        {
            UIVertex vertex = new();
            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                vertex.color = Blend(sampler.Evaluate(CalculateTime(vertex)), vertex.color);
                helper.SetUIVertex(vertex, i);
            }
        }

        private void PaintSliced(VertexHelper helper)
        {
            ReadStream(helper);
            output.Clear();
            for (int i = 0; i + TRIANGLE_VERTEX_COUNT <= stream.Count; i += TRIANGLE_VERTEX_COUNT)
                PaintTriangle(i);

            helper.Clear();
            helper.AddUIVertexTriangleStream(output);
        }

        private void ReadStream(VertexHelper helper)
        {
            stream.Clear();
            streamTimes.Clear();
            helper.GetUIVertexStream(stream);
            for (int i = 0; i < stream.Count; i++)
                streamTimes.Add(CalculateTime(stream[i]));
        }

        private void PaintTriangle(int firstIndex)
        {
            float first = streamTimes[firstIndex];
            float second = streamTimes[firstIndex + 1];
            float third = streamTimes[firstIndex + 2];
            float minimum = Mathf.Min(first, Mathf.Min(second, third));
            float maximum = Mathf.Max(first, Mathf.Max(second, third));

            polygon.Clear();
            for (int i = 0; i < TRIANGLE_VERTEX_COUNT; i++)
                polygon.Add(new TimedVertex(stream[firstIndex + i], streamTimes[firstIndex + i]));

            if (sampler.HasStopBetween(minimum, maximum))
                SlicePolygon(minimum, maximum);
            AddFan(polygon);
        }

        private void SlicePolygon(float minimumTime, float maximumTime)
        {
            for (int i = 0; i < sampler.StopCount; i++)
            {
                float stop = sampler.GetStop(i);
                if (stop <= minimumTime || stop >= maximumTime)
                    continue;

                SplitPolygonAt(stop);
                AddFan(below);
                polygon.Clear();
                polygon.AddRange(above);
            }
        }

        private void SplitPolygonAt(float stop)
        {
            below.Clear();
            above.Clear();
            for (int i = 0; i < polygon.Count; i++)
            {
                TimedVertex current = polygon[i];
                TimedVertex next = polygon[(i + 1) % polygon.Count];
                if (current.Time <= stop)
                    below.Add(current);
                if (current.Time >= stop)
                    above.Add(current);
                if ((current.Time - stop) * (next.Time - stop) < 0f)
                    AddToBothSides(TimedVertex.AtTime(current, next, stop));
            }
        }

        private void AddToBothSides(TimedVertex vertex)
        {
            below.Add(vertex);
            above.Add(vertex);
        }

        private void AddFan(List<TimedVertex> convexPolygon)
        {
            for (int i = 1; i + 1 < convexPolygon.Count; i++)
            {
                output.Add(WithGradientColor(convexPolygon[0]));
                output.Add(WithGradientColor(convexPolygon[i]));
                output.Add(WithGradientColor(convexPolygon[i + 1]));
            }
        }

        private UIVertex WithGradientColor(TimedVertex timed)
        {
            UIVertex vertex = timed.Vertex;
            vertex.color = Blend(sampler.Evaluate(timed.Time), vertex.color);
            return vertex;
        }

        /// <summary>
        /// The gradient's color with its alpha multiplied by the Graphic's own: a Graphic faded to alpha 0 stays
        /// invisible, and half-transparent text stays half-transparent. The gradient decides the hue.
        /// </summary>
        internal static Color32 Blend(Color32 gradient, Color32 graphic)
        {
            gradient.a = (byte)((gradient.a * graphic.a + 127) / 255);
            return gradient;
        }

        private float CalculateTime(UIVertex vertex)
            => sampler.CalculateTime(meshToGroup.MultiplyPoint3x4(vertex.position));
    }
}
