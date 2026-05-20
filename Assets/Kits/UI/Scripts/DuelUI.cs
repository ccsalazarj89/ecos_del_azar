using EcosDelAzar.Match;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controla el overlay visual del duelo.
/// Asigna las referencias en el Inspector.
/// </summary>
public class DuelUI : MonoBehaviour
{
    [Header("Overlay")]
    public GameObject duelOverlay;

    [Header("Cartas")]
    public Image playerCardImage;
    public Image npcCardImage;

    [Header("Resultado")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI errorText;
    public Button          continueButton;

    [Header("Dependencias")]
    public CardSpriteMapper spriteMapper;

    void Awake()
    {
        HideOverlay();
        continueButton.onClick.AddListener(HideOverlay);
    }

    void Update()
    {
        if (duelOverlay.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
            HideOverlay();
    }

    /// <summary>Muestra el overlay con las cartas y el resultado.</summary>
    public void ShowResult(Card playerCard, Card npcCard, MatchResult result, string playerId)
    {
        errorText.gameObject.SetActive(false);
        playerCardImage.gameObject.SetActive(true);
        npcCardImage.gameObject.SetActive(true);

        playerCardImage.sprite = spriteMapper.GetSprite(playerCard);
        npcCardImage.sprite    = spriteMapper.GetSprite(npcCard);

        if (result.Status == MatchResultStatus.DRAW)
        {
            resultText.text  = "EMPATE";
            resultText.color = Color.yellow;
        }
        else if (result.WinnerId == playerId)
        {
            resultText.text  = "¡GANASTE!";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text  = "PERDISTE";
            resultText.color = Color.red;
        }

        duelOverlay.SetActive(true);
    }

    /// <summary>Muestra el overlay con un mensaje de error.</summary>
    public void ShowServerError(string message = "Servidor no disponible.\nComprueba que la API está corriendo.")
    {
        playerCardImage.gameObject.SetActive(false);
        npcCardImage.gameObject.SetActive(false);
        resultText.text = string.Empty;

        errorText.text          = message;
        errorText.color         = Color.red;
        errorText.raycastTarget = false;
        errorText.gameObject.SetActive(true);

        duelOverlay.SetActive(true);
    }

    public void HideOverlay()
    {
        duelOverlay.SetActive(false);
    }
}
