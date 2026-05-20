using UnityEngine;

/// <summary>
/// Gestor global del juego. Persiste entre escenas.
/// Contiene BettingManager y BettingUI — compartidos por todos los minijuegos.
///
/// Cada minijuego (DuelManager, DiceManager, etc.) vive en su propia escena
/// y se comunica con este GameManager a través de BettingManager.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton — solo puede existir una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
