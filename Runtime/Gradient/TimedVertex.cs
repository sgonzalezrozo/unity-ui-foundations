using UnityEngine;

namespace UIFoundations
{
    /// <summary>A uGUI vertex and its position along the gradient, so slicing computes each time once.</summary>
    internal readonly struct TimedVertex
    {
        public readonly UIVertex Vertex;
        public readonly float Time;

        public TimedVertex(UIVertex vertex, float time)
        {
            Vertex = vertex;
            Time = time;
        }

        public static TimedVertex AtTime(TimedVertex from, TimedVertex to, float time)
        {
            float amount = (time - from.Time) / (to.Time - from.Time);
            return new TimedVertex(Interpolate(from.Vertex, to.Vertex, amount), time);
        }

        private static UIVertex Interpolate(UIVertex from, UIVertex to, float amount)
        {
            return new UIVertex
            {
                position = Vector3.LerpUnclamped(from.position, to.position, amount),
                normal = Vector3.LerpUnclamped(from.normal, to.normal, amount),
                tangent = Vector4.LerpUnclamped(from.tangent, to.tangent, amount),
                color = Color32.LerpUnclamped(from.color, to.color, amount),
                uv0 = Vector4.LerpUnclamped(from.uv0, to.uv0, amount),
                uv1 = Vector4.LerpUnclamped(from.uv1, to.uv1, amount),
                uv2 = Vector4.LerpUnclamped(from.uv2, to.uv2, amount),
                uv3 = Vector4.LerpUnclamped(from.uv3, to.uv3, amount)
            };
        }
    }
}
