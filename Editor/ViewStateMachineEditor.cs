using System.Collections.Generic;
using UIFoundations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ViewStateMachineRuntime = UIFoundations.ViewStateMachine;

namespace UIFoundations.Editor
{
    [CustomEditor(typeof(ViewStateMachineRuntime))]
    public class ViewStateMachineEditor : UnityEditor.Editor
    {
        private ViewStateMachineRuntime stateMachine;
        private SerializedProperty statesProperty;
        private SerializedProperty initialStateProperty;
        private bool showStates = true;

        private void OnEnable()
        {
            stateMachine = (ViewStateMachineRuntime)target;
            statesProperty = serializedObject.FindProperty("states");
            initialStateProperty = serializedObject.FindProperty("initialState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Detect Children as Views", GUILayout.Height(25)))
            {
                DetectAndSetupStates();
            }

            if (GUILayout.Button("Fix Hierarchy Order", GUILayout.Height(25)))
            {
                FixHierarchyOrder();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            DrawTransitionSettings();
            DrawInitialStateSection();
            EditorGUILayout.Space(10);
            DrawStatesSection();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void AutoDetectHierarchyChanges()
        {
            int childCount = stateMachine.transform.childCount;
            List<StateObject> states = stateMachine.GetStatesForEditor();

            bool needsUpdate = childCount != states.Count;

            if (!needsUpdate)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = stateMachine.transform.GetChild(i);
                    if (i >= states.Count || states[i].StateRoot != child.gameObject)
                    {
                        needsUpdate = true;
                        break;
                    }
                }
            }

            if (needsUpdate)
            {
                DetectAndSetupStates();
            }
        }

        private void DrawTransitionSettings()
        {
            SerializedProperty transitionTypeProperty = serializedObject.FindProperty("defaultTransitionType");
            SerializedProperty transitionDurationProperty = serializedObject.FindProperty("defaultTransitionDuration");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Transition:", GUILayout.Width(70));
            EditorGUILayout.PropertyField(transitionTypeProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            TransitionType currentType = (TransitionType)transitionTypeProperty.enumValueIndex;
            if (currentType == TransitionType.Fade)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Duration:", GUILayout.Width(70));

                int selectedPreset = GetPresetIndex(transitionDurationProperty.floatValue);
                string[] presetNames = { "Fast (0.15s)", "Normal (0.3s)", "Slow (0.6s)", "Custom" };

                int newPreset = EditorGUILayout.Popup(selectedPreset, presetNames);

                if (newPreset != selectedPreset)
                {
                    switch (newPreset)
                    {
                        case 0: transitionDurationProperty.floatValue = 0.15f; break;
                        case 1: transitionDurationProperty.floatValue = 0.3f; break;
                        case 2: transitionDurationProperty.floatValue = 0.6f; break;
                    }
                }

                if (selectedPreset == 3)
                {
                    transitionDurationProperty.floatValue = EditorGUILayout.FloatField(transitionDurationProperty.floatValue, GUILayout.Width(50));
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private int GetPresetIndex(float duration)
        {
            if (Mathf.Approximately(duration, 0.15f)) return 0;
            if (Mathf.Approximately(duration, 0.3f)) return 1;
            if (Mathf.Approximately(duration, 0.6f)) return 2;
            return 3;
        }

        private void DrawInitialStateSection()
        {
            List<string> stateNames = new();
            stateNames.Add("(None)");

            List<StateObject> states = stateMachine.GetStatesForEditor();
            foreach (StateObject state in states)
            {
                if (!state.Ignore && !string.IsNullOrEmpty(state.StateName))
                {
                    stateNames.Add(state.StateName);
                }
            }

            string currentInitialState = stateMachine.GetInitialStateForEditor();
            int currentIndex = stateNames.IndexOf(currentInitialState);
            if (currentIndex < 0)
                currentIndex = 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Initial View:", GUILayout.Width(70));
            int newIndex = EditorGUILayout.Popup(currentIndex, stateNames.ToArray());
            EditorGUILayout.EndHorizontal();

            if (newIndex != currentIndex)
            {
                string newInitialState = newIndex == 0 ? "" : stateNames[newIndex];
                stateMachine.SetInitialStateForEditor(newInitialState);
                EditorUtility.SetDirty(target);

                UpdateEditModePreview(newInitialState, states);
            }
        }

        private void UpdateEditModePreview(string initialStateName, List<StateObject> states)
        {
            if (Application.isPlaying)
                return;

            foreach (StateObject state in states)
            {
                if (state.Ignore || state.CanvasGroup == null)
                    continue;

                bool isInitialState = state.StateName == initialStateName;

                Undo.RecordObject(state.CanvasGroup, "Update Initial State Preview");

                state.CanvasGroup.alpha = isInitialState ? 1f : 0f;
                state.CanvasGroup.interactable = isInitialState;
                state.CanvasGroup.blocksRaycasts = isInitialState;

                EditorUtility.SetDirty(state.CanvasGroup);
            }
        }

        private void SetNextValidInitialState()
        {
            List<StateObject> states = stateMachine.GetStatesForEditor();

            foreach (StateObject state in states)
            {
                if (!state.Ignore && !string.IsNullOrEmpty(state.StateName))
                {
                    stateMachine.SetInitialStateForEditor(state.StateName);
                    UpdateEditModePreview(state.StateName, states);
                    EditorUtility.SetDirty(target);
                    return;
                }
            }

            stateMachine.SetInitialStateForEditor("");
            EditorUtility.SetDirty(target);
        }

        private void DrawStatesSection()
        {
            int enabledStateCount = 0;
            int disabledStateCount = 0;
            List<StateObject> states = stateMachine.GetStatesForEditor();

            foreach (StateObject state in states)
            {
                if (state.Ignore)
                    disabledStateCount++;
                else
                    enabledStateCount++;
            }

            string headerText = disabledStateCount > 0
                ? $"Views ({enabledStateCount} enabled, {disabledStateCount} disabled)"
                : $"Views ({enabledStateCount})";

            showStates = EditorGUILayout.Foldout(showStates, headerText, true, EditorStyles.foldoutHeader);

            if (!showStates)
                return;

            EditorGUI.indentLevel++;

            for (int i = 0; i < statesProperty.arraySize; i++)
            {
                DrawStateElement(i);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawStateElement(int index)
        {
            SerializedProperty stateProperty = statesProperty.GetArrayElementAtIndex(index);
            SerializedProperty stateNameProperty = stateProperty.FindPropertyRelative("StateName");
            SerializedProperty stateRootProperty = stateProperty.FindPropertyRelative("StateRoot");
            SerializedProperty canvasProperty = stateProperty.FindPropertyRelative("Canvas");
            SerializedProperty canvasGroupProperty = stateProperty.FindPropertyRelative("CanvasGroup");
            SerializedProperty graphicRaycasterProperty = stateProperty.FindPropertyRelative("GraphicRaycaster");
            SerializedProperty ignoreProperty = stateProperty.FindPropertyRelative("Ignore");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            bool isEnabled = !ignoreProperty.boolValue;

            string stateLabelText = !isEnabled
                ? $"View Item {index}: [DISABLED] {stateNameProperty.stringValue}"
                : $"View Item {index}: {stateNameProperty.stringValue}";

            bool newEnabled = EditorGUILayout.ToggleLeft(stateLabelText, isEnabled, EditorStyles.boldLabel);

            if (EditorGUI.EndChangeCheck())
            {
                bool newIgnore = !newEnabled;

                Undo.RecordObject(target, "Toggle View");
                ignoreProperty.boolValue = newIgnore;

                GameObject stateRoot = stateRootProperty.objectReferenceValue as GameObject;
                if (stateRoot != null)
                {
                    if (newIgnore)
                    {
                        RemoveStateComponents(stateRoot);

                        canvasProperty.objectReferenceValue = null;
                        canvasGroupProperty.objectReferenceValue = null;
                        graphicRaycasterProperty.objectReferenceValue = null;

                        string currentInitialState = stateMachine.GetInitialStateForEditor();
                        if (currentInitialState == stateNameProperty.stringValue)
                        {
                            SetNextValidInitialState();
                        }
                    }
                    else
                    {
                        AddStateComponents(stateRoot, canvasProperty, canvasGroupProperty, graphicRaycasterProperty);
                    }
                }

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(stateRoot);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(stateRoot.scene);

                EditorApplication.DirtyHierarchyWindowSorting();
                EditorApplication.RepaintHierarchyWindow();

                Repaint();
            }

            EditorGUI.BeginDisabledGroup(!isEnabled);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(stateNameProperty, new GUIContent("View Name"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (isEnabled && !string.IsNullOrEmpty(stateNameProperty.stringValue))
            {
                if (HasDuplicateName(stateNameProperty.stringValue, index))
                {
                    EditorGUILayout.HelpBox($"Duplicate view name '{stateNameProperty.stringValue}'! Each view must have a unique name.", MessageType.Error);
                }
            }

            EditorGUILayout.PropertyField(stateRootProperty, new GUIContent("View Root"));

            if (isEnabled)
            {
                if (!Application.isPlaying)
                {
                    if (GUILayout.Button("Preview in Editor", GUILayout.Height(24)))
                    {
                        List<StateObject> allStates = stateMachine.GetStatesForEditor();
                        UpdateEditModePreview(stateNameProperty.stringValue, allStates);
                    }
                }
                else
                {
                    if (GUILayout.Button("Switch to This View", GUILayout.Height(24)))
                    {
                        stateMachine.SetState(stateNameProperty.stringValue);
                    }
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private bool HasDuplicateName(string viewName, int currentIndex)
        {
            for (int i = 0; i < statesProperty.arraySize; i++)
            {
                if (i == currentIndex)
                    continue;

                SerializedProperty otherState = statesProperty.GetArrayElementAtIndex(i);
                SerializedProperty otherNameProperty = otherState.FindPropertyRelative("StateName");
                SerializedProperty otherIgnoreProperty = otherState.FindPropertyRelative("Ignore");

                if (!otherIgnoreProperty.boolValue && otherNameProperty.stringValue == viewName)
                {
                    return true;
                }
            }

            return false;
        }

        private void DetectAndSetupStates()
        {
            Undo.RecordObject(target, "Detect and Setup States");

            statesProperty.ClearArray();

            for (int i = 0; i < stateMachine.transform.childCount; i++)
            {
                Transform child = stateMachine.transform.GetChild(i);
                AddStateFromChild(child.gameObject);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private void FixHierarchyOrder()
        {
            List<StateObject> states = stateMachine.GetStatesForEditor();
            int fixedCount = 0;

            for (int i = 0; i < states.Count; i++)
            {
                StateObject state = states[i];
                if (state.Ignore || state.StateRoot == null)
                    continue;

                Transform stateTransform = state.StateRoot.transform;

                if (stateTransform.parent != stateMachine.transform)
                {
                    Debug.LogWarning($"[ViewStateMachine] State '{state.StateName}' is not a direct child of ViewStateMachine. Cannot fix hierarchy order.", stateMachine);
                    continue;
                }

                int currentSiblingIndex = stateTransform.GetSiblingIndex();
                if (currentSiblingIndex != i)
                {
                    Undo.RecordObject(stateTransform, "Fix Hierarchy Order");
                    stateTransform.SetSiblingIndex(i);
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                EditorUtility.SetDirty(stateMachine);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(stateMachine.gameObject.scene);
                Debug.Log($"[ViewStateMachine] Fixed hierarchy order for {fixedCount} state(s)", stateMachine);
            }
            else
            {
                Debug.Log($"[ViewStateMachine] Hierarchy order is already correct", stateMachine);
            }
        }

        private void AddStateFromChild(GameObject child)
        {
            int newIndex = statesProperty.arraySize;
            statesProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty newStateProperty = statesProperty.GetArrayElementAtIndex(newIndex);
            SerializedProperty stateNameProperty = newStateProperty.FindPropertyRelative("StateName");
            SerializedProperty stateRootProperty = newStateProperty.FindPropertyRelative("StateRoot");
            SerializedProperty canvasProperty = newStateProperty.FindPropertyRelative("Canvas");
            SerializedProperty canvasGroupProperty = newStateProperty.FindPropertyRelative("CanvasGroup");
            SerializedProperty graphicRaycasterProperty = newStateProperty.FindPropertyRelative("GraphicRaycaster");
            SerializedProperty ignoreProperty = newStateProperty.FindPropertyRelative("Ignore");

            stateNameProperty.stringValue = child.name;
            stateRootProperty.objectReferenceValue = child;
            ignoreProperty.boolValue = false;

            AddStateComponents(child, canvasProperty, canvasGroupProperty, graphicRaycasterProperty);
        }

        private void AddStateComponents(GameObject stateRoot, SerializedProperty canvasProperty, SerializedProperty canvasGroupProperty, SerializedProperty graphicRaycasterProperty)
        {
            if (stateRoot == null)
                return;

            Undo.RecordObject(stateRoot, "Add State Components");

            Canvas canvas = stateRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = Undo.AddComponent<Canvas>(stateRoot);
            }

            CanvasGroup canvasGroup = stateRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = Undo.AddComponent<CanvasGroup>(stateRoot);
            }

            GraphicRaycaster graphicRaycaster = stateRoot.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = Undo.AddComponent<GraphicRaycaster>(stateRoot);
            }

            canvasProperty.objectReferenceValue = canvas;
            canvasGroupProperty.objectReferenceValue = canvasGroup;
            graphicRaycasterProperty.objectReferenceValue = graphicRaycaster;

            HandlePrefabModifications(stateRoot);

            EditorUtility.SetDirty(stateRoot);
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveStateComponents(GameObject stateRoot)
        {
            if (stateRoot == null)
                return;

            Undo.RecordObject(stateRoot, "Remove State Components");

            GraphicRaycaster graphicRaycaster = stateRoot.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster != null)
            {
                Undo.DestroyObjectImmediate(graphicRaycaster);
            }

            CanvasGroup canvasGroup = stateRoot.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Undo.DestroyObjectImmediate(canvasGroup);
            }

            Canvas canvas = stateRoot.GetComponent<Canvas>();
            if (canvas != null)
            {
                Undo.DestroyObjectImmediate(canvas);
            }

            EditorUtility.SetDirty(stateRoot);
        }

        private void HandlePrefabModifications(GameObject gameObject)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
            }
        }

        private void ValidateStates()
        {
            List<StateObject> states = stateMachine.GetStatesForEditor();
            int errorCount = 0;
            int warningCount = 0;

            HashSet<string> stateNames = new();

            foreach (StateObject state in states)
            {
                if (state.Ignore)
                    continue;

                if (state.StateRoot == null)
                {
                    Debug.LogError($"[ViewStateMachine] State '{state.StateName}' has no StateRoot assigned", stateMachine);
                    errorCount++;
                    continue;
                }

                if (string.IsNullOrEmpty(state.StateName))
                {
                    Debug.LogWarning($"[ViewStateMachine] State with root '{state.StateRoot.name}' has no name", stateMachine);
                    warningCount++;
                }
                else if (stateNames.Contains(state.StateName))
                {
                    Debug.LogError($"[ViewStateMachine] Duplicate state name found: '{state.StateName}'", stateMachine);
                    errorCount++;
                }
                else
                {
                    stateNames.Add(state.StateName);
                }

                if (state.Canvas == null)
                {
                    Debug.LogError($"[ViewStateMachine] State '{state.StateName}' is missing Canvas component (REQUIRED)", stateMachine);
                    errorCount++;
                }

                if (state.CanvasGroup == null)
                {
                    Debug.LogError($"[ViewStateMachine] State '{state.StateName}' is missing CanvasGroup component (REQUIRED)", stateMachine);
                    errorCount++;
                }

                if (state.GraphicRaycaster == null)
                {
                    Debug.LogError($"[ViewStateMachine] State '{state.StateName}' is missing GraphicRaycaster component (REQUIRED)", stateMachine);
                    errorCount++;
                }
            }

            if (errorCount == 0 && warningCount == 0)
            {
                Debug.Log("[ViewStateMachine] All states validated successfully!", stateMachine);
            }
            else
            {
                Debug.LogWarning($"[ViewStateMachine] Validation complete: {errorCount} errors, {warningCount} warnings", stateMachine);
            }
        }
    }
}


