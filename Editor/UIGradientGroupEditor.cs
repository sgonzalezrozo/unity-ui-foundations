using UnityEditor;
using UnityEngine;

namespace UIFoundations.Editor
{
    /// <summary>
    /// The UIGradientGroup Inspector: its settings, and outside Play Mode a Refresh button that collects the group's
    /// Graphics again and repaints them, for changes the group doesn't hear about in the editor (a Graphic enabled
    /// again, a child's component added or removed).
    /// </summary>
    [CustomEditor(typeof(UIGradientGroup))]
    [CanEditMultipleObjects]
    public class UIGradientGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
                return;

            EditorGUILayout.Space(4);
            if (!GUILayout.Button(new GUIContent("Refresh", "Collects the Graphics under this group again and repaints them.")))
                return;

            foreach (Object selected in targets)
            {
                if (selected is UIGradientGroup group && group.isActiveAndEnabled)
                    group.Refresh();
            }

            SceneView.RepaintAll();
        }
    }
}
