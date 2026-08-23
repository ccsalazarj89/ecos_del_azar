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

            // Si el GameManager nace en una escena que no es el menú (p. ej. porque estás
            // probando una escena directamente desde el Editor), arrancamos ya en Playing
            // para que el HUD y el resto de sistemas de juego se comporten con normalidad.
            State = SceneManager.GetActiveScene().name == MainMenuSceneName
                ? GameState.MainMenu
                : GameState.Playing;

            ApplyStateSideEffects(State);
        }

        void OnEnable()
        {
            if (OxygenTank != null)
                OxygenTank.OnDepleted += HandlePlayerDeath;
        }

        void OnDisable()
        {
            if (OxygenTank != null)
                OxygenTank.OnDepleted -= HandlePlayerDeath;
        }

        public void SetState(GameState newState)
        {
            if (State == newState) return;

            State = newState;
            ApplyStateSideEffects(State);
            OnStateChanged?.Invoke(State);
        }

        void ApplyStateSideEffects(GameState state)
        {
            // Nadie consume oxígeno mientras el jugador está en el menú o en pausa.
            if (OxygenTank != null)
                OxygenTank.IsPaused = state == GameState.MainMenu || state == GameState.Paused;
        }

        void HandlePlayerDeath()
        {
            Debug.Log("[GameManager] Player oxygen depleted — GAME OVER.");

            // Detener el drenaje activo
            if (OxygenTank != null)
                OxygenTank.IsActiveDrain = false;

            // Resetear oxígeno para la próxima sesión
            OxygenTank?.Reset();

            // Volver al lobby
            SceneManager.LoadScene("SCN_Floor_00_Lobby");
        }
    }
}
