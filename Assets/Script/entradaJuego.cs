using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Coloca este script en el GameObject de la mesa de dados (mesadados).
/// Requiere un Collider con isTrigger = true.
/// Cuando el jugador entra en el trigger y pulsa E, carga la escena de dados.
/// </summary>
public class entradaJuego : MonoBehaviour
{
    [Header("UI")]
    public GameObject canvasDados;

    [Header("Configuración")]
    [SerializeField] string promptMessage;  //= "Pulsa E para jugar a los Dados";
    [SerializeField] string sceneName;      //= "Dice_Game"; // nombre de la escena en Build Settings

    private bool               _playerInRange;
    private PlayerInputActions _inputActions;

    void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        _inputActions.Player.Interact.performed += OnInteract;
        _inputActions.Player.Interact.Enable();
    }

    void OnDisable()
    {
        _inputActions.Player.Interact.performed -= OnInteract;
        _inputActions.Player.Interact.Disable();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!_playerInRange) return;
        SceneManager.LoadScene(sceneName);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        if (canvasDados != null) canvasDados.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        if (canvasDados != null) canvasDados.SetActive(false);
    }

    void OnGUI()
    {
        if (_playerInRange)
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 80, 200, 30), promptMessage);
    }
}
