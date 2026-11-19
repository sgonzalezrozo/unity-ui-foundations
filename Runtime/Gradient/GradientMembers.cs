using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFoundations
{
    /// <summary>
    /// Keeps a <see cref="UIGradient"/> on every Graphic a <see cref="UIGradientGroup"/> paints, and only on those.
    /// A Graphic belongs to its nearest group, unless it or a parent marked to include children has
    /// <see cref="IgnoreGroupGradient"/>.
    /// </summary>
    internal sealed class GradientMembers
    {
        private static readonly List<Graphic> pendingRemovals = new();

        private readonly List<Graphic> candidates = new();
        private readonly List<Graphic> members = new();

        public void Collect(UIGradientGroup group)
        {
            members.Clear();
            candidates.Clear();
            group.GetComponentsInChildren(true, candidates);
            for (int i = 0; i < candidates.Count; i++)
                CollectCandidate(candidates[i], group);
            candidates.Clear();
        }

        public void MarkDirty()
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] != null)
                    members[i].SetVerticesDirty();
            }
        }

        public void Release()
        {
            if (Application.isPlaying)
                RemoveEffects(members);
            else
                RemoveOrphansLater(members);
            members.Clear();
        }

        private void CollectCandidate(Graphic graphic, UIGradientGroup group)
        {
            if (graphic.GetComponentInParent<UIGradientGroup>(true) != group)
                return;

            if (IsIgnored(graphic.transform, group.transform))
            {
                RemoveEffect(graphic);
                return;
            }

            AddEffectIfMissing(graphic);
            members.Add(graphic);
        }

        private static bool IsIgnored(Transform graphic, Transform groupRoot)
        {
            for (Transform current = graphic; current != null; current = current.parent)
            {
                if (IgnoresGraphic(current, graphic))
                    return true;
                if (current == groupRoot)
                    return false;
            }
            return false;
        }

        private static bool IgnoresGraphic(Transform marked, Transform graphic)
        {
            if (!marked.TryGetComponent(out IgnoreGroupGradient ignore))
                return false;
            return marked == graphic || ignore.IgnoreMode == IgnoreGroupGradientMode.IncludeChildren;
        }

        private static void AddEffectIfMissing(Graphic graphic)
        {
            if (!graphic.TryGetComponent(out UIGradient _))
                graphic.gameObject.AddComponent<UIGradient>();
        }

        private static void RemoveEffects(List<Graphic> graphics)
        {
            for (int i = 0; i < graphics.Count; i++)
                RemoveEffect(graphics[i]);
        }

        private static void RemoveEffect(Graphic graphic)
        {
            if (graphic == null)
                return;

            if (!graphic.TryGetComponent(out UIGradient effect))
                return;

            if (Application.isPlaying)
                Object.Destroy(effect);
            else
                Object.DestroyImmediate(effect);
            graphic.SetVerticesDirty();
        }

        // In edit mode a group is also destroyed when its scene or prefab closes, and components can't be removed
        // safely then. Wait a frame, and remove only the effects left on Graphics that still exist without a group.
        private static void RemoveOrphansLater(List<Graphic> graphics)
        {
#if UNITY_EDITOR
            pendingRemovals.AddRange(graphics);
            UnityEditor.EditorApplication.delayCall -= RemoveOrphans;
            UnityEditor.EditorApplication.delayCall += RemoveOrphans;
#endif
        }

        private static void RemoveOrphans()
        {
            for (int i = 0; i < pendingRemovals.Count; i++)
            {
                Graphic graphic = pendingRemovals[i];
                if (graphic != null && graphic.GetComponentInParent<UIGradientGroup>(true) == null)
                    RemoveEffect(graphic);
            }
            pendingRemovals.Clear();
        }
    }
}
