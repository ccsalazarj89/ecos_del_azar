using EcosDelAzar.Betting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Coloca este script en el GameObject del NPC o mesa interactuable.
/// Requiere un Collider con isTrigger = true para detectar al jugador.
/// Al pulsar E carga la escena del minijuego indicada.
/// </summary>
public class DuelNPC : MonoBehaviour
{
    [Header("Configuración")]
    public string     promptMessage = "Pulsa E para jugar";
    public string     targetScene   = "HighCardScene"; // escena del minijuego a cargar

    [Header("UI (opcional)")]
    public GameObject promptCanvas; // canvas/panel que se muestra al entrar en rango

    private bool               _playerInRange = false;
    private BettingManager     _bettingManager;
    private PlayerInputActions _inputActions;

    void Awake()
    {
        _bettingManager = FindFirstObjectByType<BettingManager>();
        _inputActions   = new PlayerInputActions();

        if (_bettingManager == null)
            Debug.LogError("[DuelNPC] No se encontró BettingManager en la escena.");
    }

    void OnEnable()
    {
        if (_inputActions == null) _inputActions = new PlayerInputActions();
        _inputActions.Player.Interact.performed += OnInteract;
        _inputActions.Player.Interact.Enable();
    }

    void OnDisable()
    {
        if (_inputActions == null) return;
        _inputActions.Player.Interact.performed -= OnInteract;
        _inputActions.Player.Interact.Disable();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!_playerInRange) return;

        if (_bettingManager != null)
        {
            if (_bettingManager.PlayerChips < _bettingManager.minimumBet)
            {
                Debug.Log("[DuelNPC] El jugador no tiene fichas suficientes.");
                return;
            }
            if (_bettingManager.NpcChips < _bettingManager.minimumBet)
            {
                Debug.Log("[DuelNPC] El NPC no tiene fichas suficientes.");
                return;
            }
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        if (promptCanvas != null) promptCanvas.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        if (promptCanvas != null) promptCanvas.SetActive(false);
    }

    void OnGUI()
    {
        if (!_playerInRange) return;

        string mensaje = promptMessage;

        if (_bettingManager != null)
        {
            if (_bettingManager.PlayerChips < _bettingManager.minimumBet)
                mensaje = "No tienes fichas suficientes";
            else if (_bettingManager.NpcChips < _bettingManager.minimumBet)
                mensaje = "El NPC no tiene fichas";
        }

        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 80, 200, 30), mensaje);
    }
}
