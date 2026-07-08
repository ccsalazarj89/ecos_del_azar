using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcosDelAzar.Core
{
    [RequireComponent(typeof(Wallet))]
    [RequireComponent(typeof(OxygenTank))]
    [RequireComponent(typeof(FloorProgress))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public Wallet Wallet { get; private set; }
        public OxygenTank OxygenTank { get; private set; }
        public FloorProgress FloorProgress { get; private set; }

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
