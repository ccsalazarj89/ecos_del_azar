using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using EcosDelAzar.MiniGames;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DuelUI : MonoBehaviour
    {
        [SerializeField] InputActionReference closeAction;

        VisualElement root;
        Label resultLabel;
        Label playerCardLabel;
        Label opponentCardLabel;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>().rootVisualElement;
            root = doc.Q("duel-overlay");
            resultLabel = doc.Q<Label>("result-text");
            playerCardLabel = doc.Q<Label>("player-card");
            opponentCardLabel = doc.Q<Label>("opponent-card");

            doc.Q<Button>("btn-continue").clicked += Hide;
            Hide();

            if (closeAction?.action != null)
            {
                closeAction.action.performed += _ => Hide();
                closeAction.action.Enable();
            }
        }

        public void ShowResult(Card playerCard, Card opponentCard, MatchResult result, string playerId)
        {
            playerCardLabel.text = playerCard.ToString();
            opponentCardLabel.text = opponentCard.ToString();

            if (result.Status == MatchResultStatus.Draw)
            {
                resultLabel.text = "EMPATE";
            }
            else if (result.WinnerId == playerId)
            {
                resultLabel.text = "GANASTE!";
            }
            else
            {
                resultLabel.text = "PERDISTE";
            }

            root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            root.style.display = DisplayStyle.None;
        }
    }
}
