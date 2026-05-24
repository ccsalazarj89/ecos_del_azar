using System;
using System.Collections;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public event Action<DiceResult> OnRollFinished;

    [Header("Dice View")]
    [SerializeField] private DiceView diceView;

    [Header("Roll Settings")]
    [SerializeField] private float rollDuration = 1.2f;
    [SerializeField] private float frameInterval = 0.05f;

    private Coroutine rollCoroutine;
    private bool isRolling;

    public bool IsRolling => isRolling;

    public void Roll()
    {
        if (isRolling)
            return;

        rollCoroutine = StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        if (diceView == null)
        {
            Debug.LogError("DiceRoller needs a DiceView assigned.", this);
            yield break;
        }

        isRolling = true;

        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            float normalizedTime = elapsed / rollDuration;

            diceView.SetRandomFace();
            diceView.AnimateRolling(normalizedTime);

            yield return new WaitForSeconds(frameInterval);

            elapsed += frameInterval;
        }

        int finalValue = UnityEngine.Random.Range(1, 7);

        diceView.SetFace(finalValue);
        diceView.ResetVisuals();

        isRolling = false;
        rollCoroutine = null;

        OnRollFinished?.Invoke(new DiceResult(finalValue));
    }

    private void OnDisable()
    {
        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);
            rollCoroutine = null;
        }

        isRolling = false;
    }
}