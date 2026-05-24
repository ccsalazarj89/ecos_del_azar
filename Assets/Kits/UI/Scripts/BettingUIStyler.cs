using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aplica estilo visual de poker al panel de apuestas.
/// Añade este script al GameObject BettingPanel y asigna las referencias.
/// </summary>
public class BettingUIStyler : MonoBehaviour
{
    [Header("Panel")]
    public Image panelBackground;

    [Header("Textos")]
    public TextMeshProUGUI minimumBetText;
    public TextMeshProUGUI playerChipsText;
    public TextMeshProUGUI npcChipsText;
    public TextMeshProUGUI npcBetText;

    [Header("Botones")]
    public Button equalButton;
    public Button doubleButton;
    public Button allInButton;
    public Button foldButton;
    public Button abandonButton;

    // ── Paleta poker ─────────────────────────────────────────
    static readonly Color PanelBg        = new Color(0.07f, 0.18f, 0.07f, 0.95f); // verde oscuro mesa
    static readonly Color PanelBorder    = new Color(0.72f, 0.58f, 0.18f, 1f);    // dorado

    static readonly Color BtnEqual       = new Color(0.15f, 0.55f, 0.25f, 1f);    // verde
    static readonly Color BtnDouble      = new Color(0.80f, 0.40f, 0.05f, 1f);    // naranja
    static readonly Color BtnAllIn       = new Color(0.70f, 0.10f, 0.10f, 1f);    // rojo
    static readonly Color BtnFold        = new Color(0.30f, 0.30f, 0.35f, 1f);    // gris
    static readonly Color BtnAbandon     = new Color(0.12f, 0.12f, 0.15f, 0.80f); // negro sutil

    static readonly Color TextGold       = new Color(0.95f, 0.82f, 0.35f, 1f);    // dorado
    static readonly Color TextWhite      = new Color(0.95f, 0.95f, 0.95f, 1f);
    static readonly Color TextNpcBet     = new Color(1f,    0.65f, 0.10f, 1f);    // naranja NPC
    static readonly Color TextBtnLight   = new Color(0.95f, 0.95f, 0.90f, 1f);

    void Start() => Apply();

    [ContextMenu("Aplicar estilo")]
    public void Apply()
    {
        StylePanel();
        StyleTexts();
        StyleButton(equalButton,   "IGUALAR",     BtnEqual);
        StyleButton(doubleButton,  "DOBLAR",      BtnDouble);
        StyleButton(allInButton,   "ALL IN",      BtnAllIn);
        StyleButton(foldButton,    "RETIRARSE",   BtnFold);
        StyleButton(abandonButton, "SALIR",       BtnAbandon);
    }

    private void StylePanel()
    {
        if (panelBackground == null) return;
        panelBackground.color = PanelBg;

        // Borde dorado vía Outline si existe, o lo añadimos
        var outline = panelBackground.GetComponent<Outline>();
        if (outline == null) outline = panelBackground.gameObject.AddComponent<Outline>();
        outline.effectColor     = PanelBorder;
        outline.effectDistance  = new Vector2(3, 3);
    }

    private void StyleTexts()
    {
        SetText(playerChipsText, 18, TextGold,   FontStyles.Bold);
        SetText(npcChipsText,    18, TextGold,   FontStyles.Bold);
        SetText(npcBetText,      20, TextNpcBet, FontStyles.Bold);
        SetText(minimumBetText,  14, TextWhite,  FontStyles.Normal);
    }

    private void StyleButton(Button btn, string label, Color bgColor)
    {
        if (btn == null) return;

        // Fondo
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = bgColor;

        // ColorBlock para hover/press
        var cb              = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor * 1.25f;
        cb.pressedColor     = bgColor * 0.75f;
        cb.selectedColor    = bgColor;
        cb.fadeDuration     = 0.1f;
        btn.colors          = cb;

        // Texto
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text       = label;
            tmp.color      = TextBtnLight;
            tmp.fontSize   = 14;
            tmp.fontStyle  = FontStyles.Bold;
        }
    }

    private void SetText(TextMeshProUGUI tmp, float size, Color color, FontStyles style)
    {
        if (tmp == null) return;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
    }
}
