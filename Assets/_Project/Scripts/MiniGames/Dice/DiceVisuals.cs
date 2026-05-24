using UnityEngine;

namespace EcosDelAzar.MiniGames.Dice
{
    public class DiceVisuals : MonoBehaviour
    {
        [SerializeField] DiceGame game;
        [SerializeField] SpriteRenderer playerDiceRenderer;
        [SerializeField] SpriteRenderer opponentDiceRenderer;
        [SerializeField] Sprite[] faceSprites = new Sprite[6];
        [SerializeField] float rotationSpeed = 720f;
        [SerializeField] float maxScaleMultiplier = 1.15f;

        Vector3 playerOriginalScale;
        Vector3 opponentOriginalScale;
        bool isAnimating;

        void Awake()
        {
            playerOriginalScale = playerDiceRenderer.transform.localScale;
            opponentOriginalScale = opponentDiceRenderer.transform.localScale;
        }

        void OnEnable()
        {
            game.OnRolling += AnimateFrame;
            game.OnRollFinished += ShowFinalResult;
            game.OnRoundStarted += OnRoundStart;
        }

        void OnDisable()
        {
            game.OnRolling -= AnimateFrame;
            game.OnRollFinished -= ShowFinalResult;
            game.OnRoundStarted -= OnRoundStart;
        }

        void OnRoundStart()
        {
            isAnimating = true;
        }

        void AnimateFrame(int playerVal, int opponentVal)
        {
            SetFace(playerDiceRenderer, playerVal);
            SetFace(opponentDiceRenderer, opponentVal);

            playerDiceRenderer.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            opponentDiceRenderer.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            float scale = maxScaleMultiplier;
            playerDiceRenderer.transform.localScale = playerOriginalScale * scale;
            opponentDiceRenderer.transform.localScale = opponentOriginalScale * scale;
        }

        void ShowFinalResult(int playerVal, int opponentVal)
        {
            isAnimating = false;
            SetFace(playerDiceRenderer, playerVal);
            SetFace(opponentDiceRenderer, opponentVal);
            ResetTransforms();
        }

        void SetFace(SpriteRenderer renderer, int value)
        {
            if (value < 1 || value > faceSprites.Length) return;
            renderer.sprite = faceSprites[value - 1];
        }

        void ResetTransforms()
        {
            playerDiceRenderer.transform.localScale = playerOriginalScale;
            playerDiceRenderer.transform.rotation = Quaternion.identity;
            opponentDiceRenderer.transform.localScale = opponentOriginalScale;
            opponentDiceRenderer.transform.rotation = Quaternion.identity;
        }
    }
}
