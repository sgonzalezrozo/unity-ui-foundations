using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFoundations
{
    public class ViewStateMachine : MonoBehaviour
    {
        public Action OnStateChanged;

        [SerializeField] private List<StateObject> states = new();
        [SerializeField] private string initialState;
        [SerializeField] private TransitionType defaultTransitionType = TransitionType.Instant;
        [SerializeField] private float defaultTransitionDuration = 0.3f;

        private StateObject currentState;
        private IStateMachineTransition currentTransition;
        private bool isTransitioning;

        // Performance optimization: O(1) lookups
        private Dictionary<string, int> stateNameToIndex;
        private int currentStateIndex = -1;
        private bool isInitialized;

        public string CurrentStateName { get; private set; }
        public int CurrentStateIndex => currentStateIndex;
        public bool IsTransitioning => isTransitioning;

        private void Start()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (isInitialized)
                return;

            InitializeStates();
            isInitialized = true;
        }

        private void InitializeStates()
        {
            // Build O(1) lookup cache
            stateNameToIndex = new Dictionary<string, int>(states.Count);

            for (int i = 0; i < states.Count; i++)
            {
                StateObject state = states[i];
                if (state.Ignore)
                    continue;

                if (!string.IsNullOrEmpty(state.StateName))
                {
                    if (stateNameToIndex.ContainsKey(state.StateName))
                    {
                        Debug.LogError($"[ViewStateMachine] Duplicate state name found: {state.StateName}");
                        continue;
                    }
                    stateNameToIndex[state.StateName] = i;
                }
            }

            if (string.IsNullOrEmpty(initialState))
            {
                if (states.Count > 0 && !states[0].Ignore)
                {
                    initialState = states[0].StateName;
                }
            }

            for (int i = 0; i < states.Count; i++)
            {
                StateObject state = states[i];
                if (state.Ignore)
                    continue;

                if (!state.IsValid())
                {
                    Debug.LogError($"[ViewStateMachine] Invalid state configuration: {state.StateName}");
                    continue;
                }

                bool isInitial = state.StateName == initialState;
                state.SetActiveInstant(isInitial);

                if (isInitial)
                {
                    currentState = state;
                    currentStateIndex = i;
                    CurrentStateName = state.StateName;
                }

                // Ensure state GameObjects maintain correct hierarchy order for proper rendering
                EnsureStateHierarchyOrder(state, i);
            }

            if (currentState == null && states.Count > 0)
            {
                Debug.LogWarning("[ViewStateMachine] No valid initial state found");
            }
        }

        private void EnsureStateHierarchyOrder(StateObject state, int expectedIndex)
        {
            if (state.StateRoot == null)
                return;

            Transform stateTransform = state.StateRoot.transform;

            // Verify state is a child of this ViewStateMachine
            if (stateTransform.parent != transform)
            {
                Debug.LogWarning($"[ViewStateMachine] State '{state.StateName}' is not a direct child of ViewStateMachine. Rendering order may be incorrect.");
                return;
            }

            // Ensure sibling index matches the state index for consistent rendering order
            int currentSiblingIndex = stateTransform.GetSiblingIndex();
            if (currentSiblingIndex != expectedIndex)
            {
                stateTransform.SetSiblingIndex(expectedIndex);
            }
        }

        public void SetState(string stateName)
        {
            IStateMachineTransition transition = CreateTransitionFromType(defaultTransitionType, defaultTransitionDuration);
            SetState(stateName, transition);
        }

        public void SetState(string stateName, IStateMachineTransition transition)
        {
            EnsureInitialized();

            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogError("[ViewStateMachine] Cannot set state with null or empty name");
                return;
            }

            // O(1) lookup using cached index
            int newStateIndex = GetStateIndex(stateName);
            if (newStateIndex < 0)
            {
                Debug.LogError($"[ViewStateMachine] State not found: {stateName}");
                return;
            }

            SetStateByIndex(newStateIndex, transition);
        }

        public void SetState(int stateIndex)
        {
            IStateMachineTransition transition = CreateTransitionFromType(defaultTransitionType, defaultTransitionDuration);
            SetState(stateIndex, transition);
        }

        public void SetState(int stateIndex, IStateMachineTransition transition)
        {
            EnsureInitialized();
            SetStateByIndex(stateIndex, transition);
        }

        private void SetStateByIndex(int newStateIndex, IStateMachineTransition transition)
        {
            if (newStateIndex < 0 || newStateIndex >= states.Count)
            {
                Debug.LogError($"[ViewStateMachine] State index out of range: {newStateIndex}");
                return;
            }

            if (currentStateIndex == newStateIndex)
                return;

            StateObject newState = states[newStateIndex];

            if (newState.Ignore)
            {
                Debug.LogWarning($"[ViewStateMachine] Cannot activate ignored state at index: {newStateIndex}");
                return;
            }

            if (!newState.IsValid())
            {
                Debug.LogError($"[ViewStateMachine] Cannot activate invalid state at index: {newStateIndex}");
                return;
            }

            if (isTransitioning)
            {
                currentTransition?.Cancel();
                isTransitioning = false;
            }

            StateObject previousState = currentState;
            currentState = newState;
            currentStateIndex = newStateIndex;
            CurrentStateName = newState.StateName;
            currentTransition = transition;

            PerformTransition(previousState, newState, transition);
        }

        public void SetStateInstant(string stateName)
        {
            SetState(stateName, new InstantTransition());
        }

        public void SetStateInstant(int stateIndex)
        {
            SetState(stateIndex, new InstantTransition());
        }

        public bool IsInState(string stateName)
        {
            return CurrentStateName == stateName;
        }

        public StateObject GetState(string stateName)
        {
            EnsureInitialized();

            if (stateNameToIndex != null && stateNameToIndex.TryGetValue(stateName, out int index))
            {
                return states[index];
            }
            return null;
        }

        public StateObject GetState(int index)
        {
            EnsureInitialized();

            if (index >= 0 && index < states.Count)
            {
                return states[index];
            }
            return null;
        }

        private int GetStateIndex(string stateName)
        {
            EnsureInitialized();

            if (stateNameToIndex != null && stateNameToIndex.TryGetValue(stateName, out int index))
            {
                return index;
            }
            return -1;
        }

        public GameObject GetCurrentStateGameObject()
        {
            return currentState?.StateRoot;
        }

        public Transform GetCurrentStateTransform()
        {
            return currentState?.StateRoot?.transform;
        }

        private void PerformTransition(StateObject from, StateObject to, IStateMachineTransition transition)
        {
            isTransitioning = true;

            if (from != null && !from.Ignore)
            {
                from.Hide(transition, () =>
                {
                    to.Show(transition, () =>
                    {
                        isTransitioning = false;
                        OnStateChanged?.Invoke();
                    });
                });
            }
            else
            {
                to.Show(transition, () =>
                {
                    isTransitioning = false;
                    OnStateChanged?.Invoke();
                });
            }
        }

        private void OnDestroy()
        {
            if (isTransitioning)
            {
                currentTransition?.Cancel();
                isTransitioning = false;
            }
        }

        private IStateMachineTransition CreateTransitionFromType(TransitionType type, float duration)
        {
            switch (type)
            {
                case TransitionType.Instant:
                    return new InstantTransition();
                case TransitionType.Fade:
                    return new FadeTransition(duration);
                default:
                    return new InstantTransition();
            }
        }

#if UNITY_EDITOR
        public List<StateObject> GetStatesForEditor()
        {
            return states;
        }

        public void SetInitialStateForEditor(string stateName)
        {
            initialState = stateName;
        }

        public string GetInitialStateForEditor()
        {
            return initialState;
        }
#endif
    }
}


