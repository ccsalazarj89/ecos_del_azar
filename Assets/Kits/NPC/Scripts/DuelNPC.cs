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
    private DuelManager _duelManager;

    void Awake()
    {
        _duelManager = FindFirstObjectByType<DuelManager>();

        if (_duelManager == null)
            Debug.LogError("[DuelNPC] No se encontró DuelManager en la escena. Asegúrate de que el GameManager tiene el componente DuelManager.");
    }

    void Update()
    {
        if (_duelManager == null) return;
        if (_playerInRange && Keyboard.current[Key.E].wasPressedThisFrame && !_duelManager.DuelInProgress)
        {
            _duelManager.StartDuel(npcId);
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
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 80, 200, 30), promptMessage);
        }
    }
}
