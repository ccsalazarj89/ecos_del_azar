using EcosDelAzar.Betting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Coloca este script en el GameObject del NPC.
/// Requiere un Collider con isTrigger = true para detectar al jugador.
/// </summary>
public class DuelNPC : MonoBehaviour
{
    [Header("Configuración")]
    public string npcId = "b2c3d4e5-f6a7-8901-bcde-f12345678901"; // UUID del NPC como jugador
    public string interactKey = "e";
    public string promptMessage = "Pulsa E para duelo";

    private bool _playerInRange = false;
    private DuelManager    _duelManager;
    private BettingUI      _bettingUI;
    private BettingManager _bettingManager;

    void Awake()
    {
        _duelManager    = FindFirstObjectByType<DuelManager>();
        _bettingUI      = FindFirstObjectByType<BettingUI>();
        _bettingManager = FindFirstObjectByType<BettingManager>();

        if (_duelManager == null)
            Debug.LogError("[DuelNPC] No se encontró DuelManager en la escena.");
        if (_bettingUI == null)
            Debug.LogError("[DuelNPC] No se encontró BettingUI en la escena.");
        if (_bettingManager == null)
            Debug.LogError("[DuelNPC] No se encontró BettingManager en la escena.");
    }

    void Update()
    {
        if (_duelManager == null) return;
        if (_playerInRange && Keyboard.current[Key.E].wasPressedThisFrame && !_duelManager.DuelInProgress)
        {
            if (_bettingManager != null && _bettingManager.PlayerChips < _bettingManager.minimumBet)
            {
                Debug.Log("[DuelNPC] El jugador no tiene fichas suficientes para apostar.");
                return;
            }

            _duelManager.StartDuel(npcId);
            _bettingUI?.ShowPanel();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = false;
    }

    void OnGUI()
    {
        if (_duelManager == null) return;
        if (_playerInRange && !_duelManager.DuelInProgress)
        {
            bool sinFichas = _bettingManager != null &&
                             _bettingManager.PlayerChips < _bettingManager.minimumBet;

            string mensaje = sinFichas ? "No hay fichas suficientes" : promptMessage;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 80, 200, 30), mensaje);
        }
    }
}
