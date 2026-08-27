using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Esc while playing (and no world panel open) pauses: time stops, oxygen
    /// stops, and the menu offers Continue / Main menu / Quit. The run is saved
    /// continuously, so "main menu" simply leaves; Continuar in the title screen
    /// brings the player back to the last floor. Lives on the HUD document.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuUI : MonoBehaviour
    {
        const string HiddenClass = "pause--hidden";
        const string MainMenuSceneName = "SCN_MainMenu";

        VisualElement overlay;
        Button btnResume;
        Button btnMenu;
        Button btnQuit;

        public bool IsPaused { get; private set; }

        void OnEnable()
        {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null) return;

            overlay = root.Q("PauseOverlay");
            btnResume = root.Q<Button>("btn-pause-resume");
            btnMenu = root.Q<Button>("btn-pause-menu");
            btnQuit = root.Q<Button>("btn-pause-quit");

            if (btnResume != null) btnResume.clicked += Resume;
            if (btnMenu != null) btnMenu.clicked += ToMainMenu;
            if (btnQuit != null) btnQuit.clicked += Quit;

#if UNITY_WEBGL
            // Browsers own the tab; quitting the application means nothing there.
            if (btnQuit != null) btnQuit.style.display = DisplayStyle.None;
#endif
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            if (btnResume != null) btnResume.clicked -= Resume;
            if (btnMenu != null) btnMenu.clicked -= ToMainMenu;
            if (btnQuit != null) btnQuit.clicked -= Quit;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

            if (IsPaused) { Resume(); return; }

            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            if (ModalTracker.IsAnyOpen) return;

            Pause();
        }

        void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            GameManager.Instance.SetState(GameState.Paused);
            overlay?.RemoveFromClassList(HiddenClass);
        }

        void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            overlay?.AddToClassList(HiddenClass);
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused)
                GameManager.Instance.SetState(GameState.Playing);
        }

        void ToMainMenu()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            overlay?.AddToClassList(HiddenClass);
            GameManager.Instance?.SetState(GameState.MainMenu);
            SceneManager.LoadScene(MainMenuSceneName);
        }

        void Quit()
        {
            Time.timeScale = 1f;
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Panels are destroyed with their scene without calling Close.
            ModalTracker.Reset();
        }
    }
}
