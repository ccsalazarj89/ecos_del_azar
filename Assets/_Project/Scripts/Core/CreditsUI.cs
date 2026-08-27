using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Credits overlay on the HUD document: shown after beating the boss and
    /// from the title screen. Content lives in GameHUD.uxml (static text).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CreditsUI : MonoBehaviour
    {
        const string HiddenClass = "credits--hidden";

        VisualElement overlay;
        Button button;
        Action onDone;

        public bool IsOpen { get; private set; }

        void OnEnable()
        {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null) return;

            overlay = root.Q("CreditsOverlay");
            button = root.Q<Button>("btn-credits-close");
            if (button != null) button.clicked += Close;
        }

        void OnDisable()
        {
            if (button != null) button.clicked -= Close;
        }

        public void Show(Action done)
        {
            onDone = done;
            IsOpen = true;
            overlay?.RemoveFromClassList(HiddenClass);
        }

        void Close()
        {
            IsOpen = false;
            overlay?.AddToClassList(HiddenClass);
            var cb = onDone;
            onDone = null;
            cb?.Invoke();
        }
    }
}
