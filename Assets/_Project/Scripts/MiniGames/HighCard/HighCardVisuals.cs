using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.MiniGames.HighCard
{
    public class HighCardVisuals : MonoBehaviour
    {
        [SerializeField] HighCardGame game;
        [SerializeField] UIDocument uiDocument;
        [SerializeField] CardSpriteMapper spriteMapper;
        [SerializeField] Sprite cardBackSprite;

        VisualElement playerCardImg;
        VisualElement opponentCardImg;

        void OnEnable()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                playerCardImg = uiDocument.rootVisualElement.Q<VisualElement>("player-card-img");
                opponentCardImg = uiDocument.rootVisualElement.Q<VisualElement>("opponent-card-img");
            }

            if (game != null)
            {
                game.OnRoundStarted += ResetCards;
                game.OnPlayerCardDrawn += ShowPlayerCard;
                game.OnOpponentCardRevealed += ShowOpponentCard;
            }

            ResetCards();
        }

        void OnDisable()
        {
            if (game != null)
            {
                game.OnRoundStarted -= ResetCards;
                game.OnPlayerCardDrawn -= ShowPlayerCard;
                game.OnOpponentCardRevealed -= ShowOpponentCard;
            }
        }

        void ResetCards()
        {
            SetCard(playerCardImg, cardBackSprite);
            SetCard(opponentCardImg, cardBackSprite);
        }

        void ShowPlayerCard(Card card) => SetCard(playerCardImg, spriteMapper.GetSprite(card));

        void ShowOpponentCard(Card card) => SetCard(opponentCardImg, spriteMapper.GetSprite(card));

        void SetCard(VisualElement el, Sprite sprite)
        {
            if (el == null || sprite == null) return;
            el.style.backgroundImage = new StyleBackground(sprite);
        }
    }
}
