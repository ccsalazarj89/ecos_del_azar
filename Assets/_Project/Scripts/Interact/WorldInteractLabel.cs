using UnityEngine;
using TMPro;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class WorldInteractLabel : MonoBehaviour
    {
        [SerializeField] string hintText = "[E] Interactuar";
        [SerializeField] string blockedText = "No disponible";
        [Tooltip("Optional. Leave empty to auto-find the IInteractable on this object or any parent.")]
        [SerializeField] MonoBehaviour triggerSource;
        [SerializeField] float blockedDuration = 2f;
        [SerializeField] bool faceCamera = true;

        [SerializeField] Color hintColor = new Color(0.80f, 0.77f, 0.69f);
        [SerializeField] Color blockedColor = new Color(0.92f, 0.37f, 0.27f);

        TMP_Text label;
        IInteractable interactable;
        Transform cam;

        float blockedTimer;
        bool showingBlocked;

        void Awake()
        {
            label = GetComponent<TMP_Text>();
            interactable = ResolveInteractable();
        }

        // Use the explicit reference if set, otherwise discover the IInteractable
        // on this object or any parent. This lets a base prefab carry the label
        // while variants add their own IInteractable (e.g. MinigameEntryTrigger)
        // without re-wiring anything.
        IInteractable ResolveInteractable()
        {
            if (triggerSource is IInteractable explicitSource) return explicitSource;
            return GetComponentInParent<IInteractable>(true);
        }

        void OnEnable()
        {
            CacheCamera();
            SetVisible(false);

            if (interactable != null)
            {
                interactable.OnPlayerRangeChanged += OnRangeChanged;
                interactable.OnInteractionBlocked += OnBlocked;
                interactable.OnInteractionStarted += OnInteractionStarted;
            }
        }

        void OnDisable()
        {
            if (interactable != null)
            {
                interactable.OnPlayerRangeChanged -= OnRangeChanged;
                interactable.OnInteractionBlocked -= OnBlocked;
                interactable.OnInteractionStarted -= OnInteractionStarted;
            }
        }

        void LateUpdate()
        {
            if (faceCamera && label.enabled)
            {
                if (cam == null) CacheCamera();
                if (cam != null) transform.rotation = cam.rotation;
            }

            if (!showingBlocked) return;

            blockedTimer -= Time.deltaTime;
            if (blockedTimer <= 0f)
            {
                showingBlocked = false;
                RefreshDisplay();
            }
        }

        void OnRangeChanged(bool inRange)
        {
            showingBlocked = false;

            if (inRange)
            {
                RefreshDisplay();
                SetVisible(true);
            }
            else
            {
                SetVisible(false);
            }
        }

        void OnBlocked()
        {
            showingBlocked = true;
            blockedTimer = blockedDuration;
            RefreshDisplay();
            SetVisible(true);
        }

        void OnInteractionStarted()
        {
            showingBlocked = false;
            SetVisible(false);
        }

        void RefreshDisplay()
        {
            if (label == null) return;

            string hint = interactable?.HintOverride ?? hintText;
            label.text = showingBlocked ? blockedText : hint;
            label.color = showingBlocked ? blockedColor : hintColor;
        }

        void SetVisible(bool visible)
        {
            if (label != null) label.enabled = visible;
        }

        void CacheCamera()
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null) cam = mainCamera.transform;
        }
    }
}
