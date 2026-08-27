using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using EcosDelAzar.Audio;
using EcosDelAzar.Core.Echoes;

namespace EcosDelAzar.Core
{
    [RequireComponent(typeof(Wallet))]
    [RequireComponent(typeof(OxygenTank))]
    [RequireComponent(typeof(FloorProgress))]
    [RequireComponent(typeof(MusicPlayer))]
    public class GameManager : MonoBehaviour
    {
        const string MainMenuSceneName = "SCN_MainMenu";
        const string HubScenePrefix = "SCN_Floor_";

        [SerializeField] EcoCatalog ecoCatalog;

        public static GameManager Instance { get; private set; }

        public Wallet Wallet { get; private set; }
        public OxygenTank OxygenTank { get; private set; }
        public FloorProgress FloorProgress { get; private set; }
        public MusicPlayer Music { get; private set; }
        public RunModifiers Modifiers { get; private set; }

        public GameState State { get; private set; } = GameState.MainMenu;

        public event Action<GameState> OnStateChanged;

        RunEndUI runEndUI;
        bool runEnding;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Wallet = GetComponent<Wallet>();
            OxygenTank = GetComponent<OxygenTank>();
            FloorProgress = GetComponent<FloorProgress>();
            Music = GetComponent<MusicPlayer>();
            Modifiers = new RunModifiers(ecoCatalog);
            runEndUI = GetComponentInChildren<RunEndUI>(true);

            // Born outside the menu (testing a scene from the Editor): behave as if
            // a run were in progress so HUD and drain work normally.
            bool inMenu = SceneManager.GetActiveScene().name == MainMenuSceneName;
            State = inMenu ? GameState.MainMenu : GameState.Playing;
            if (!inMenu && !RunState.Exists) RunState.StartNew();

            ApplyStateSideEffects(State);
        }

        void OnEnable()
        {
            if (OxygenTank != null)
                OxygenTank.OnDepleted += HandlePlayerDeath;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        void OnDisable()
        {
            if (OxygenTank != null)
                OxygenTank.OnDepleted -= HandlePlayerDeath;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public void SetState(GameState newState)
        {
            if (State == newState) return;

            State = newState;
            ApplyStateSideEffects(State);
            OnStateChanged?.Invoke(State);
        }

        /// <summary>Starting values of a fresh run. Called by RunState.StartNew after the prefs are wiped.</summary>
        public void ResetRunValues()
        {
            Modifiers?.Reload();
            Wallet?.ResetToInitial();
            OxygenTank?.Reset();
            if (OxygenTank != null) OxygenTank.IsActiveDrain = false;
            runEnding = false;
        }

        /// <summary>
        /// Ends the run with a full-screen message. The run data is wiped when the
        /// player dismisses it, so the main menu comes back without "Continuar".
        /// </summary>
        public void EndRun(string title, string subtitle)
        {
            if (runEnding) return;
            runEnding = true;

            if (OxygenTank != null) OxygenTank.IsActiveDrain = false;
            SetState(GameState.Paused);

            if (runEndUI == null)
            {
                FinishRun();
                return;
            }

            runEndUI.Show(title, subtitle, "VOLVER AL MENÚ", FinishRun);
        }

        void FinishRun()
        {
            RunState.Clear();
            runEnding = false;
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(MainMenuSceneName);
        }

        void ApplyStateSideEffects(GameState state)
        {
            // Nobody breathes oxygen while in the menu or paused.
            if (OxygenTank != null)
                OxygenTank.IsPaused = state == GameState.MainMenu || state == GameState.Paused;
        }

        void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Hub floors are the resume points of a run; minigame scenes are not.
            if (scene.name.StartsWith(HubScenePrefix))
                RunState.MarkScene(scene.name);
        }

        void HandlePlayerDeath()
        {
            // "Bombona de reserva": one Echo buys a second chance, once per run.
            float revive = Modifiers?.TryConsumeRevive() ?? 0f;
            if (revive > 0f && OxygenTank != null)
            {
                OxygenTank.Restore(OxygenTank.Max * revive);
                return;
            }

            Debug.Log("[GameManager] Player oxygen depleted — run over.");
            EndRun("TE HAS QUEDADO SIN AIRE", "El casino se queda con todo lo que ganaste. La próxima vez, respira antes de apostar.");
        }
    }
}
