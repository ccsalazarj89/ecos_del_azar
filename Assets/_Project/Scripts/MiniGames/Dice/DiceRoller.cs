using System;
using System.Collections;
using UnityEngine;

namespace EcosDelAzar.MiniGames
{
    public class DiceRoller : MonoBehaviour
    {
        [SerializeField] DiceView diceView;
        [SerializeField] float rollDuration = 1.2f;
        [SerializeField] float frameInterval = 0.05f;

        public event Action<DiceResult> OnRollFinished;
        public bool IsRolling { get; private set; }

        Coroutine rollCoroutine;

        public void Roll()
        {
            if (IsRolling) return;
            rollCoroutine = StartCoroutine(RollRoutine());
        }

        IEnumerator RollRoutine()
        {
            if (diceView == null) yield break;

            IsRolling = true;
            float elapsed = 0f;

            while (elapsed < rollDuration)
            {
                float t = elapsed / rollDuration;
                diceView.SetRandomFace();
                diceView.AnimateRolling(t);
                yield return new WaitForSeconds(frameInterval);
                elapsed += frameInterval;
            }

            int finalValue = UnityEngine.Random.Range(1, 7);
            diceView.SetFace(finalValue);
            diceView.ResetVisuals();

            IsRolling = false;
            rollCoroutine = null;
            OnRollFinished?.Invoke(new DiceResult(finalValue));
        }

        void OnDisable()
        {
            if (rollCoroutine != null)
            {
                StopCoroutine(rollCoroutine);
                rollCoroutine = null;
            }
            IsRolling = false;
        }
    }
}
