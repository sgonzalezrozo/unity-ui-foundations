using System;

namespace UIFoundations
{
    public class InstantTransition : IStateMachineTransition
    {
        public void Show(StateObject state, Action onComplete)
        {
            if (state?.CanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            state.CanvasGroup.alpha = 1f;
            state.CanvasGroup.interactable = true;
            state.CanvasGroup.blocksRaycasts = true;

            onComplete?.Invoke();
        }

        public void Hide(StateObject state, Action onComplete)
        {
            if (state?.CanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            state.CanvasGroup.alpha = 0f;
            state.CanvasGroup.interactable = false;
            state.CanvasGroup.blocksRaycasts = false;

            onComplete?.Invoke();
        }

        public void Cancel()
        {
        }
    }
}


