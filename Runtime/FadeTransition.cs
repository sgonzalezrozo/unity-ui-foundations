using System;
using System.Collections;
using UnityEngine;

namespace UIFoundations
{
    public class FadeTransition : IStateMachineTransition
    {
        private const float DEFAULT_DURATION = 0.3f;

        private readonly float duration;
        private readonly AnimationCurve curve;
        private Coroutine activeCoroutine;
        private MonoBehaviour coroutineRunner;

        public FadeTransition(float duration = DEFAULT_DURATION)
        {
            this.duration = Mathf.Max(0f, duration);
            curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        public FadeTransition(float duration, AnimationCurve customCurve)
        {
            this.duration = Mathf.Max(0f, duration);
            curve = customCurve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        public void Show(StateObject state, Action onComplete)
        {
            if (state?.CanvasGroup == null || state.StateRoot == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (duration <= 0f)
            {
                state.CanvasGroup.alpha = 1f;
                state.CanvasGroup.interactable = true;
                state.CanvasGroup.blocksRaycasts = true;
                onComplete?.Invoke();
                return;
            }

            coroutineRunner = GetOrCreateCoroutineRunner(state.StateRoot);
            activeCoroutine = coroutineRunner.StartCoroutine(FadeIn(state, onComplete));
        }

        public void Hide(StateObject state, Action onComplete)
        {
            if (state?.CanvasGroup == null || state.StateRoot == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (duration <= 0f)
            {
                state.CanvasGroup.alpha = 0f;
                state.CanvasGroup.interactable = false;
                state.CanvasGroup.blocksRaycasts = false;
                onComplete?.Invoke();
                return;
            }

            coroutineRunner = GetOrCreateCoroutineRunner(state.StateRoot);
            activeCoroutine = coroutineRunner.StartCoroutine(FadeOut(state, onComplete));
        }

        public void Cancel()
        {
            if (activeCoroutine != null && coroutineRunner != null)
            {
                coroutineRunner.StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }
        }

        private MonoBehaviour GetOrCreateCoroutineRunner(GameObject stateRoot)
        {
            TransitionCoroutineRunner runner = stateRoot.GetComponent<TransitionCoroutineRunner>();
            if (runner == null)
            {
                runner = stateRoot.AddComponent<TransitionCoroutineRunner>();
            }
            return runner;
        }

        private IEnumerator FadeIn(StateObject state, Action onComplete)
        {
            float elapsed = 0f;
            float startAlpha = state.CanvasGroup.alpha;

            state.CanvasGroup.interactable = false;
            state.CanvasGroup.blocksRaycasts = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = curve.Evaluate(t);
                state.CanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
                yield return null;
            }

            state.CanvasGroup.alpha = 1f;
            state.CanvasGroup.interactable = true;
            state.CanvasGroup.blocksRaycasts = true;

            activeCoroutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator FadeOut(StateObject state, Action onComplete)
        {
            float elapsed = 0f;
            float startAlpha = state.CanvasGroup.alpha;

            state.CanvasGroup.interactable = false;
            state.CanvasGroup.blocksRaycasts = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = curve.Evaluate(t);
                state.CanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
                yield return null;
            }

            state.CanvasGroup.alpha = 0f;

            activeCoroutine = null;
            onComplete?.Invoke();
        }

        public static FadeTransition Fast => new(0.15f);
        public static FadeTransition Normal => new(0.3f);
        public static FadeTransition Slow => new(0.6f);
    }
}


