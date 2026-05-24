using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.UI
{
    public interface IInteractable
    {
        event Action<bool> OnPlayerRangeChanged;
        event Action OnInteractionBlocked;
    }

    [RequireComponent(typeof(UIDocument))]
    public class InteractHintUI : MonoBehaviour
    {
        [SerializeField] string hintText = "[E] Interactuar";
        [SerializeField] string blockedText = "No disponible";
        [SerializeField] MonoBehaviour triggerSource;
        [SerializeField] float blockedDuration = 2f;

        VisualElement root;
        Label hintLabel;

        const string HiddenClass = "interact-hint--hidden";
        const string WarningClass = "interact-hint--warning";

        float blockedTimer;
        bool showingBlocked;
        IInteractable interactable;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            root = doc.rootVisualElement.Q("interact-hint-root");
            hintLabel = doc.rootVisualElement.Q<Label>("interact-hint-text");

            interactable = triggerSource as IInteractable;

            Hide();

            if (interactable != null)
            {
                interactable.OnPlayerRangeChanged += OnRangeChanged;
                interactable.OnInteractionBlocked += OnBlocked;
            }
        }

        void OnDisable()
        {
            if (interactable != null)
            {
                interactable.OnPlayerRangeChanged -= OnRangeChanged;
                interactable.OnInteractionBlocked -= OnBlocked;
            }
        }

        void Update()
        {
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
            if (inRange)
            {
                showingBlocked = false;
                RefreshDisplay();
                Show();
            }
            else
            {
                showingBlocked = false;
                Hide();
            }
        }

        void OnBlocked()
        {
            showingBlocked = true;
            blockedTimer = blockedDuration;
            RefreshDisplay();
            Show();
        }

        void RefreshDisplay()
        {
            if (hintLabel == null || root == null) return;

            if (showingBlocked)
            {
                hintLabel.text = blockedText;
                root.AddToClassList(WarningClass);
            }
            else
            {
                hintLabel.text = hintText;
                root.RemoveFromClassList(WarningClass);
            }
        }

        void Show()
        {
            if (root != null) root.RemoveFromClassList(HiddenClass);
        }

        void Hide()
        {
            if (root != null) root.AddToClassList(HiddenClass);
        }
    }
}
