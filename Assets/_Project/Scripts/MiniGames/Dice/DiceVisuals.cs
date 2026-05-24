using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.MiniGames.Dice
{
    public class DiceVisuals : MonoBehaviour
    {
        [SerializeField] DiceGame game;
        [SerializeField] UIDocument uiDocument;
        [SerializeField] Sprite[] diceFaces;

        [Header("Animation Settings")]
        [SerializeField] float rotationSpeed = 360f;
        [SerializeField] float scaleBump = 1.2f;

        VisualElement playerDiceImg;
        VisualElement opponentDiceImg;

        void OnEnable()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                playerDiceImg = uiDocument.rootVisualElement.Q<VisualElement>("player-dice-img");
                opponentDiceImg = uiDocument.rootVisualElement.Q<VisualElement>("opponent-dice-img");
            }

            if (game != null)
            {
                game.OnRolling += HandleRolling;
                game.OnRollFinished += HandleRollFinished;
            }

            // Set initial blank or face 1
            SetFace(playerDiceImg, 1);
            SetFace(opponentDiceImg, 1);
        }

        void OnDisable()
        {
            if (game != null)
            {
                game.OnRolling -= HandleRolling;
                game.OnRollFinished -= HandleRollFinished;
            }
        }

        void HandleRolling(int playerVal, int opponentVal)
        {
            if (this == null || !gameObject.activeInHierarchy) return;

            SetFace(playerDiceImg, playerVal);
            SetFace(opponentDiceImg, opponentVal);

            // Calculate rotation and scale based on time to create a "wobble"
            float angle = Mathf.Sin(Time.time * rotationSpeed * Mathf.Deg2Rad) * 45f;
            float scale = 1f + Mathf.PingPong(Time.time * 5f, scaleBump - 1f);

            ApplyTransform(playerDiceImg, angle, scale);
            ApplyTransform(opponentDiceImg, angle, scale);
        }

        void HandleRollFinished(int playerVal, int opponentVal)
        {
            if (this == null || !gameObject.activeInHierarchy) return;

            SetFace(playerDiceImg, playerVal);
            SetFace(opponentDiceImg, opponentVal);

            // Reset rotation and scale
            ApplyTransform(playerDiceImg, 0f, 1f);
            ApplyTransform(opponentDiceImg, 0f, 1f);
        }

        void SetFace(VisualElement el, int faceValue)
        {
            if (el == null || diceFaces == null || diceFaces.Length == 0) return;
            
            int index = Mathf.Clamp(faceValue - 1, 0, diceFaces.Length - 1);
            el.style.backgroundImage = new StyleBackground(diceFaces[index]);
        }

        void ApplyTransform(VisualElement el, float angle, float scale)
        {
            if (el == null) return;
            el.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
            el.style.scale = new Scale(new Vector2(scale, scale));
        }
    }
}
