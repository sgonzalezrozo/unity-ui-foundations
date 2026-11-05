using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFoundations
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class BaseClickController : Graphic, IPointerClickHandler
    {
        [SerializeField] private bool interactable = true;

        private bool isInitialized;

        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                raycastTarget = value;
            }
        }

#if UNITY_EDITOR
        private new void Reset()
        {
            Color c = color;
            c.a = 0f;
            color = c;
            raycastTarget = interactable;
        }

        protected override void OnValidate()
        {
            Color c = color;
            c.a = 0f;
            color = c;
            raycastTarget = interactable;
        }
#endif

        protected override void Start()
        {
            base.Start();

            isInitialized = InitializeController();
            if (isInitialized)
                return;

            Debug.LogError($"[{GetType().Name}] Controller initialization failed on {gameObject.name}", gameObject);
            enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInitialized)
                return;

            if (!interactable)
                return;

            OnClick();
        }

        protected virtual bool InitializeController()
        {
            return true;
        }

        protected abstract void OnClick();
    }
}
