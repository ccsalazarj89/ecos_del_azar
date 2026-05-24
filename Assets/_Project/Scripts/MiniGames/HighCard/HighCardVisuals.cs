using UnityEngine;

namespace EcosDelAzar.MiniGames.HighCard
{
    public class HighCardVisuals : MonoBehaviour
    {
        [SerializeField] HighCardGame game;
        [SerializeField] CardSpriteMapper spriteMapper;
        [SerializeField] SpriteRenderer playerCardRenderer;
        [SerializeField] SpriteRenderer opponentCardRenderer;
        [SerializeField] Sprite cardBackSprite;

        void OnEnable()
        {
            game.OnRoundStarted += ResetCards;
            game.OnPlayerCardDrawn += ShowPlayerCard;
            game.OnOpponentCardRevealed += ShowOpponentCard;
        }

        void OnDisable()
        {
            game.OnRoundStarted -= ResetCards;
            game.OnPlayerCardDrawn -= ShowPlayerCard;
            game.OnOpponentCardRevealed -= ShowOpponentCard;
        }

        void ResetCards()
        {
            playerCardRenderer.sprite = cardBackSprite;
            opponentCardRenderer.sprite = cardBackSprite;
        }

        void ShowPlayerCard(Card card)
        {
            playerCardRenderer.sprite = spriteMapper.GetSprite(card);
        }

        void ShowOpponentCard(Card card)
        {
            opponentCardRenderer.sprite = spriteMapper.GetSprite(card);
        }
    }
}
