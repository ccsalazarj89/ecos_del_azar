using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using EcosDelAzar.Core;

namespace EcosDelAzar.UI
{
    /// <summary>
    /// Main menu: NUEVO JUEGO / CONTINUAR / SALIR. "Continuar" only shows while a
    /// run exists; "Nuevo juego" asks for a second click when it would discard one.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuUI : MonoBehaviour
    {
        const string NewGameText = "NUEVO JUEGO";
        const string ConfirmText = "¿BORRAR PARTIDA?";
        const float ConfirmWindow = 3f;

        Button newGameButton;
        Button continueButton;
        Button quitButton;
        Button creditsButton;

        bool awaitingConfirm;
        Coroutine confirmTimer;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            var root = doc.rootVisualElement;
            newGameButton = root.Q<Button>("NewGameButton");
            continueButton = root.Q<Button>("ContinueButton");
            quitButton = root.Q<Button>("QuitButton");
            creditsButton = root.Q<Button>("CreditsButton");

            if (newGameButton != null) newGameButton.clicked += OnNewGameClicked;
            if (continueButton != null) continueButton.clicked += Continue;
            if (quitButton != null) quitButton.clicked += Quit;
            if (creditsButton != null) creditsButton.clicked += ShowCredits;
#if UNITY_WEBGL
            // Browsers own the tab; there is nothing to quit.
            if (quitButton != null) quitButton.style.display = DisplayStyle.None;
#endif

            Refresh();
        }

        void OnDisable()
        {
            if (newGameButton != null) newGameButton.clicked -= OnNewGameClicked;
            if (continueButton != null) continueButton.clicked -= Continue;
            if (quitButton != null) quitButton.clicked -= Quit;
            if (creditsButton != null) creditsButton.clicked -= ShowCredits;
        }

        void Update()
        {
            // Esc on the title screen quits; inside the game it closes panels instead.
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Quit();
        }

        void Refresh()
        {
            bool hasRun = RunState.Exists;

            if (continueButton != null)
                continueButton.style.display = hasRun ? DisplayStyle.Flex : DisplayStyle.None;

            if (newGameButton != null)
            {
                newGameButton.text = NewGameText;
                newGameButton.EnableInClassList("menu-button--primary", !hasRun);
                newGameButton.EnableInClassList("menu-button--danger", false);
            }

            if (continueButton != null)
                continueButton.EnableInClassList("menu-button--primary", hasRun);

            awaitingConfirm = false;
        }

        void OnNewGameClicked()
        {
            if (RunState.Exists && !awaitingConfirm)
            {
                // First click only warns; the run is discarded on the second one.
                awaitingConfirm = true;
                newGameButton.text = ConfirmText;
                newGameButton.EnableInClassList("menu-button--danger", true);
                if (confirmTimer != null) StopCoroutine(confirmTimer);
                confirmTimer = StartCoroutine(ResetConfirmAfterDelay());
                return;
            }

            RunState.StartNew();
            EnterGame(RunState.CurrentScene);
        }

        void Continue()
        {
            if (!RunState.Exists) { Refresh(); return; }
            EnterGame(RunState.CurrentScene);
        }

        void EnterGame(string sceneName)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);

            SceneManager.LoadScene(sceneName);
        }

        IEnumerator ResetConfirmAfterDelay()
        {
            yield return new WaitForSecondsRealtime(ConfirmWindow);
            Refresh();
        }

        void ShowCredits() => GameManager.Instance?.ShowCredits();

        void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
