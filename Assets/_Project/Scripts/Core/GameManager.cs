using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using EcosDelAzar.Audio;

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

        public static GameManager Instance { get; private set; }

        public Wallet Wallet { get; private set; }
        public OxygenTank OxygenTank { get; private set; }
        public FloorProgress FloorProgress { get; private set; }
        public MusicPlayer Music { get; private set; }

        public GameState State { get; private set; } = GameState.MainMenu;

        public event Action<GameState> OnStateChanged;

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
            Wallet?.ResetToInitial();
            OxygenTank?.Reset();
            if (OxygenTank != null) OxygenTank.IsActiveDrain = false;
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
            Debug.Log("[GameManager] Player oxygen depleted — run over.");

            if (OxygenTank != null)
                OxygenTank.IsActiveDrain = false;

            // Permadeath: the run is gone and "Continuar" disappears from the menu.
            RunState.Clear();
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}
