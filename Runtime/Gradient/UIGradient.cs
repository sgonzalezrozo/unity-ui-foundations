using UnityEngine;
using UnityEngine.UI;

namespace UIFoundations
{
    /// <summary>
    /// Paints one Graphic with the gradient of its nearest <see cref="UIGradientGroup"/>.
    /// The group adds and removes this component and hides it in the Inspector; don't add it yourself.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class UIGradient : BaseMeshEffect
    {
        private UIGradientGroup group;

        protected override void Awake()
        {
            base.Awake();
            hideFlags = HideFlags.HideInInspector;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            group = null;
        }

        public override void ModifyMesh(VertexHelper helper)
        {
            if (!IsActive() || helper.currentVertCount == 0)
                return;

            if (group == null)
                group = GetComponentInParent<UIGradientGroup>();
            if (group == null || !group.IsPainting)
                return;

            group.Paint(helper, transform);
        }
    }
}
