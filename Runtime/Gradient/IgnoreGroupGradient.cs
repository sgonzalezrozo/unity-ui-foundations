using UnityEngine;
using UnityEngine.Serialization;

namespace UIFoundations
{
    public enum IgnoreGroupGradientMode
    {
        SelfOnly,
        IncludeChildren
    }

    /// <summary>
    /// Leaves a Graphic out of its parent <see cref="UIGradientGroup"/>, alone or together with its children.
    /// </summary>
    [DisallowMultipleComponent]
    public class IgnoreGroupGradient : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("_ignoreMode")]
        [Tooltip("Whether to ignore only this GameObject, or this GameObject and all its children.")]
        private IgnoreGroupGradientMode ignoreMode = IgnoreGroupGradientMode.SelfOnly;

        public IgnoreGroupGradientMode IgnoreMode
        {
            get => ignoreMode;
            set => ignoreMode = value;
        }
    }
}
