using System.Collections.Generic;
using UnityEngine;

namespace UIFoundations
{
    /// <summary>
    /// Maps a point in a <see cref="UIGradientGroup"/>'s local space to a gradient time and a color.
    /// Reads the gradient only when it changes, into a color table and a list of inner key times, so painting a vertex
    /// is a table lookup and resizing or rotating never allocates.
    /// </summary>
    internal sealed class GradientSampler
    {
        private const float STOP_MERGE_DISTANCE = 0.0001f;
        private const int COLOR_TABLE_SIZE = 256;
        private const int LAST_COLOR_INDEX = COLOR_TABLE_SIZE - 1;

        private readonly List<float> stops = new();
        private readonly Color32[] colorTable = new Color32[COLOR_TABLE_SIZE];
        private Vector2 direction = Vector2.right;
        private float minimumProjection;
        private float inverseRange;

        public int StopCount
            => stops.Count;

        public float GetStop(int index)
            => stops[index];

        public void SetGradient(Gradient source)
        {
            if (source == null)
                throw new System.ArgumentNullException(nameof(source));

            FillColorTable(source);
            stops.Clear();
            AddInnerTimes(source.colorKeys);
            AddInnerTimes(source.alphaKeys);
            stops.Sort();
            RemoveDuplicateStops();
        }

        public void SetFrame(float angleDegrees, Rect groupRect)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            UpdateProjectionRange(groupRect);
        }

        public float CalculateTime(Vector3 groupPoint)
            => (groupPoint.x * direction.x + groupPoint.y * direction.y - minimumProjection) * inverseRange;

        public Color32 Evaluate(float time)
            => colorTable[(int)(Mathf.Clamp01(time) * LAST_COLOR_INDEX + 0.5f)];

        public bool HasStopBetween(float minimumTime, float maximumTime)
        {
            for (int i = 0; i < stops.Count; i++)
            {
                if (stops[i] > minimumTime && stops[i] < maximumTime)
                    return true;
            }
            return false;
        }

        private void UpdateProjectionRange(Rect groupRect)
        {
            float bottomLeft = Project(groupRect.xMin, groupRect.yMin);
            float bottomRight = Project(groupRect.xMax, groupRect.yMin);
            float topLeft = Project(groupRect.xMin, groupRect.yMax);
            float topRight = Project(groupRect.xMax, groupRect.yMax);

            minimumProjection = Mathf.Min(Mathf.Min(bottomLeft, bottomRight), Mathf.Min(topLeft, topRight));
            float maximum = Mathf.Max(Mathf.Max(bottomLeft, bottomRight), Mathf.Max(topLeft, topRight));
            float range = maximum - minimumProjection;
            inverseRange = range > 0f ? 1f / range : 0f;
        }

        private float Project(float x, float y)
            => x * direction.x + y * direction.y;

        private void FillColorTable(Gradient source)
        {
            for (int i = 0; i < COLOR_TABLE_SIZE; i++)
                colorTable[i] = source.Evaluate(i / (float)LAST_COLOR_INDEX);
        }

        private void AddInnerTimes(GradientColorKey[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
                AddIfInner(keys[i].time);
        }

        private void AddInnerTimes(GradientAlphaKey[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
                AddIfInner(keys[i].time);
        }

        private void AddIfInner(float time)
        {
            if (time > 0f && time < 1f)
                stops.Add(time);
        }

        private void RemoveDuplicateStops()
        {
            for (int i = stops.Count - 1; i > 0; i--)
            {
                if (stops[i] - stops[i - 1] < STOP_MERGE_DISTANCE)
                    stops.RemoveAt(i);
            }
        }
    }
}
