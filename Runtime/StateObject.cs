using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFoundations
{
    [Serializable]
    public class StateObject
    {
        public string StateName;
        public GameObject StateRoot;
        public Canvas Canvas;
        public CanvasGroup CanvasGroup;
        public GraphicRaycaster GraphicRaycaster;
        public bool Ignore;

        public bool IsActive { get; private set; }

        public void Show(IStateMachineTransition transition, Action onComplete = null)
        {
            if (Ignore)
            {
                Debug.LogWarning($"[ViewStateMachine] Cannot show ignored state: {StateName}");
                onComplete?.Invoke();
                return;
            }

            if (CanvasGroup == null)
            {
                Debug.LogError($"[ViewStateMachine] CanvasGroup missing on state: {StateName}");
                onComplete?.Invoke();
                return;
            }

            IsActive = true;
            transition.Show(this, onComplete);
        }

        public void Hide(IStateMachineTransition transition, Action onComplete = null)
        {
            if (Ignore)
            {
                onComplete?.Invoke();
                return;
            }

            if (CanvasGroup == null)
            {
                Debug.LogError($"[ViewStateMachine] CanvasGroup missing on state: {StateName}");
                onComplete?.Invoke();
                return;
            }

            IsActive = false;
            transition.Hide(this, onComplete);
        }

        public void SetActiveInstant(bool active)
        {
            if (Ignore || CanvasGroup == null)
                return;

            IsActive = active;

            if (active)
            {
                CanvasGroup.alpha = 1f;
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;
            }
            else
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
            }
        }

        public bool IsValid()
        {
            if (Ignore)
                return true;

            return StateRoot != null && Canvas != null && CanvasGroup != null && GraphicRaycaster != null;
        }
    }
}


