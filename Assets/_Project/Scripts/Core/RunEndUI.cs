using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Full-screen "run is over" panel (death or victory). Lives on the HUD
    /// UIDocument inside the GameManager prefab so it is available in every scene.
    /// GameManager shows it and decides what happens when the button is pressed.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class RunEndUI : MonoBehaviour
    {
        const string HiddenClass = "run-end--hidden";

        VisualElement overlay;
        Label title;
        Label subtitle;
        Button button;

        Action onConfirm;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null) return;

            overlay = root.Q("RunEndOverlay");
            title = root.Q<Label>("run-end-title");
            subtitle = root.Q<Label>("run-end-subtitle");
            button = root.Q<Button>("run-end-button");

            if (button != null) button.clicked += Confirm;
        }

        void OnDisable()
        {
            if (button != null) button.clicked -= Confirm;
        }

        public void Show(string titleText, string subtitleText, string buttonText, Action confirm)
        {
            onConfirm = confirm;
            if (title != null) title.text = titleText;
            if (subtitle != null) subtitle.text = subtitleText;
            if (button != null) button.text = buttonText;
            overlay?.RemoveFromClassList(HiddenClass);
        }

        public void Hide() => overlay?.AddToClassList(HiddenClass);

        void Confirm()
        {
            Hide();
            var cb = onConfirm;
            onConfirm = null;
            cb?.Invoke();
        }
    }
}
