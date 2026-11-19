using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIFoundations.Editor
{
    /// <summary>
    /// Right-click in the Hierarchy > UI Foundations:
    /// <list type="bullet">
    /// <item><b>Click Controller…</b> asks for a class name, writes a <see cref="BaseClickController"/> subclass in the folder
    /// open in the Project window and, once Unity has compiled it, adds it on a new child of the selected object.</item>
    /// <item><b>View State Machine (On, Off)</b> adds a <see cref="ViewStateMachine"/> with two states, On and Off, ready to
    /// fill and switch with <c>SetState("On")</c>.</item>
    /// </list>
    /// </summary>
    [InitializeOnLoad]
    internal static class FoundationsHierarchyMenu
    {
        private const string MENU = "GameObject/UI Foundations/";
        private const int PRIORITY = 10;
        private const string PENDING_KEY = "UIFoundations.PendingClickController";
        private static readonly string[] OnOffStates = { "On", "Off" };

        [Serializable]
        private class PendingController
        {
            public string className;
            public string parentId;
        }

        static FoundationsHierarchyMenu()
            => EditorApplication.delayCall += AddPendingController;

        // ---------- Click Controller ----------

        [MenuItem(MENU + "Click Controller…", false, PRIORITY)]
        private static void NewClickController(MenuCommand command)
        {
            GameObject parent = command.context as GameObject ?? Selection.activeGameObject;
            string folder = ActiveProjectFolder();
            NamePromptWindow.Show(
                "New Click Controller",
                "Class Name",
                "NewClickController",
                $"Writes {folder}/<Name>.cs and adds it on a new child of {(parent != null ? parent.name : "the scene")} after the compile.",
                name => CreateClickController(ToIdentifier(name), folder, parent));
        }

        private static void CreateClickController(string className, string folder, GameObject parent)
        {
            string parentId = parent != null ? GlobalObjectId.GetGlobalObjectIdSlow(parent).ToString() : string.Empty;

            // A controller with that name already compiled: add it now.
            Type existing = FindController(className);
            if (existing != null)
            {
                AddController(existing, parent);
                return;
            }

            string path = $"{folder}/{className}.cs";
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("UI Foundations", $"'{path}' already exists.", "OK");
                return;
            }

            File.WriteAllText(path, ClickControllerTemplate(className));
            SessionState.SetString(PENDING_KEY, JsonUtility.ToJson(new PendingController { className = className, parentId = parentId }));
            AssetDatabase.Refresh();
        }

        private static void AddPendingController()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AddPendingController;
                return;
            }

            string json = SessionState.GetString(PENDING_KEY, string.Empty);
            if (string.IsNullOrEmpty(json))
                return;

            PendingController pending = JsonUtility.FromJson<PendingController>(json);
            Type type = FindController(pending.className);
            if (type == null)
            {
                // Still compiling, or the script doesn't compile: keep it for the next reload.
                if (EditorUtility.scriptCompilationFailed)
                    Debug.LogWarning($"[UI Foundations] {pending.className} will be added once the project compiles. Fix the compile errors in the Console.");
                return;
            }

            SessionState.EraseString(PENDING_KEY);
            GameObject parent = null;
            if (GlobalObjectId.TryParse(pending.parentId, out GlobalObjectId id))
                parent = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
            AddController(type, parent);
        }

        private static void AddController(Type type, GameObject parent)
        {
            GameObject go = NewUIObject(type.Name, parent);
            Stretch((RectTransform)go.transform);
            Undo.AddComponent(go, type);
            Selection.activeGameObject = go;
        }

        private static string ClickControllerTemplate(string className) =>
$@"using UIFoundations;
using UnityEngine;

/// <summary>
/// Handles taps on this object. It's an invisible raycast target: size its RectTransform to the area that takes taps.
/// Set Interactable to false to ignore taps (it also turns raycasts off).
/// </summary>
public class {className} : BaseClickController
{{
    protected override void OnClick()
    {{
        Debug.Log(""[{className}] Clicked"", this);
    }}
}}
";

        // ---------- View State Machine ----------

        [MenuItem(MENU + "View State Machine (On, Off)", false, PRIORITY + 1)]
        private static void NewOnOffStateMachine(MenuCommand command)
        {
            GameObject parent = command.context as GameObject ?? Selection.activeGameObject;
            GameObject root = NewUIObject("StateMachine", parent);
            Stretch((RectTransform)root.transform);
            ViewStateMachine machine = Undo.AddComponent<ViewStateMachine>(root);

            SerializedObject serialized = new(machine);
            SerializedProperty states = serialized.FindProperty("states");
            states.ClearArray();
            foreach (string stateName in OnOffStates)
            {
                GameObject state = NewUIObject(stateName, root);
                Stretch((RectTransform)state.transform);
                Canvas canvas = Undo.AddComponent<Canvas>(state);
                CanvasGroup group = Undo.AddComponent<CanvasGroup>(state);
                GraphicRaycaster raycaster = Undo.AddComponent<GraphicRaycaster>(state);

                int index = states.arraySize;
                states.InsertArrayElementAtIndex(index);
                SerializedProperty element = states.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("StateName").stringValue = stateName;
                element.FindPropertyRelative("StateRoot").objectReferenceValue = state;
                element.FindPropertyRelative("Canvas").objectReferenceValue = canvas;
                element.FindPropertyRelative("CanvasGroup").objectReferenceValue = group;
                element.FindPropertyRelative("GraphicRaycaster").objectReferenceValue = raycaster;
                element.FindPropertyRelative("Ignore").boolValue = false;
            }

            serialized.FindProperty("initialState").stringValue = "Off";
            serialized.ApplyModifiedProperties();
            Selection.activeGameObject = root;
        }

        // ---------- Helpers ----------

        /// <summary>A UI object under <paramref name="parent"/> (or at the scene root), on the UI layer, with Undo.</summary>
        private static GameObject NewUIObject(string name, GameObject parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
                go.layer = uiLayer;

            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            if (parent != null)
                GameObjectUtility.SetParentAndAlign(go, parent);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Type FindController(string className)
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<BaseClickController>())
            {
                if (type.Name == className && !type.IsAbstract)
                    return type;
            }

            return null;
        }

        private static string ToIdentifier(string name)
        {
            System.Text.StringBuilder builder = new();
            foreach (char c in name ?? string.Empty)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    builder.Append(c);
            }

            if (builder.Length == 0)
                return "NewClickController";
            if (char.IsDigit(builder[0]))
                builder.Insert(0, '_');
            builder[0] = char.ToUpperInvariant(builder[0]);
            return builder.ToString();
        }

        /// <summary>The folder open in the Project window, or Assets.</summary>
        private static string ActiveProjectFolder()
        {
            MethodInfo method = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            if (method != null)
            {
                object[] args = { null };
                if ((bool)method.Invoke(null, args) && args[0] is string active && AssetDatabase.IsValidFolder(active) && active.StartsWith("Assets", StringComparison.Ordinal))
                    return active;
            }

            return "Assets";
        }
    }
}
