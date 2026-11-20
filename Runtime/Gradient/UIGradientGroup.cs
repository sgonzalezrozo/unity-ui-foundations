using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIFoundations
{
    /// <summary>
    /// Spreads one gradient across every Graphic under this object, as if they were a single shape.
    /// Each Graphic gets a hidden <see cref="UIGradient"/> that colors its vertices by where they sit in this
    /// object's rectangle. Nothing runs per frame: the gradient is applied when uGUI rebuilds a mesh.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIGradientGroup : MonoBehaviour
    {
        private const float MAXIMUM_ANGLE = 360f;

        private static readonly GradientMeshPainter painter = new();

        [SerializeField]
        [FormerlySerializedAs("gradientValue")]
        private Gradient gradient = new();

        [SerializeField]
        [Range(0f, MAXIMUM_ANGLE)]
        [FormerlySerializedAs("gradientAngleValue")]
        private float angle;

        private readonly GradientSampler sampler = new();
        private readonly GradientMembers members = new();
        private RectTransform rectTransform;

        public Gradient Gradient
        {
            get => gradient;
            set
            {
                gradient = value ?? throw new ArgumentNullException(nameof(value));
                sampler.SetGradient(gradient);
                members.MarkDirty();
            }
        }

        public float Angle
        {
            get => angle;
            set
            {
                angle = Mathf.Repeat(value, MAXIMUM_ANGLE);
                UpdateFrame();
                members.MarkDirty();
            }
        }

        internal bool IsPainting
            => isActiveAndEnabled;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
        }

        private void OnEnable()
        {
            Refresh();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
#endif
            members.MarkDirty();
        }

        private void OnDestroy()
        {
            members.Release();
        }

        // Only fires for direct children. Graphics added deeper are picked up by Refresh(), and in the
        // Editor by OnHierarchyChanged.
        private void OnTransformChildrenChanged()
        {
            if (!isActiveAndEnabled)
                return;

            members.Collect(this);
            members.MarkDirty();
        }

#if UNITY_EDITOR
        // Tools that rebuild nested children (e.g. a Graphic created under a grandchild) don't trigger
        // OnTransformChildrenChanged on the group, so the new Graphic would be left unpainted.
        private void OnHierarchyChanged()
        {
            if (this == null || !isActiveAndEnabled)
                return;

            if (members.Collect(this))
                members.MarkDirty();
        }
#endif

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
                return;

            UpdateFrame();
            members.MarkDirty();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            sampler.SetGradient(gradient);
            UpdateFrame();
            members.MarkDirty();
        }
#endif

        public void Refresh()
        {
            members.Collect(this);
            sampler.SetGradient(gradient);
            UpdateFrame();
            members.MarkDirty();
        }

        internal void Paint(VertexHelper helper, Transform member)
        {
            Matrix4x4 memberToGroup = transform.worldToLocalMatrix * member.localToWorldMatrix;
            painter.Paint(helper, sampler, memberToGroup);
        }

        private void UpdateFrame()
        {
            if (rectTransform == null)
                rectTransform = (RectTransform)transform;
            sampler.SetFrame(angle, rectTransform.rect);
        }
    }
}
