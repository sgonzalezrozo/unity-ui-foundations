using UIFoundations;
using UnityEditor;

namespace UIFoundations.Editor
{
    [CustomEditor(typeof(BaseClickController), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class BaseClickControllerEditor : UnityEditor.Editor
    {
        private BaseClickController myScript;

        private void OnEnable()
        {
            myScript ??= (BaseClickController)target;
        }

        public override void OnInspectorGUI()
        {
            myScript ??= (BaseClickController)target;
            EditorGUI.BeginChangeCheck();

            myScript.Interactable = EditorGUILayout.Toggle("Interactable", myScript.Interactable);

            EditorGUI.EndChangeCheck();
        }
    }
}
