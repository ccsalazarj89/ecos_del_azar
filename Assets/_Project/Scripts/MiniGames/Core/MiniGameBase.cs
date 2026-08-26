using System;
using System.Collections;
using UnityEngine;

namespace EcosDelAzar.MiniGames
{
    public enum MiniGameState { Idle, WaitingForBet, Playing, Resolved }

    public abstract class MiniGameBase : MonoBehaviour
    {
        public MiniGameState State { get; private set; } = MiniGameState.Idle;
        public RoundResult LastResult { get; private set; }
        public bool InProgress => State != MiniGameState.Idle;

        /// <summary>Shown by the betting panel while the round is being played.</summary>
        public virtual string PlayingStatusText => "Jugando...";

        public event Action OnRoundStarted;
        public event Action<RoundResult> OnRoundResolved;
        public event Action OnReadyForNextRound;
        public event Action OnMatchEnded;

        public void Begin()
        {
            if (InProgress) return;
            State = MiniGameState.WaitingForBet;
            OnBegin();
        }

        public void PlayRound()
        {
            if (State != MiniGameState.WaitingForBet) return;
            State = MiniGameState.Playing;
            OnRoundStarted?.Invoke();
            StartCoroutine(ExecuteRound());
        }

        public void End()
        {
            StopAllCoroutines();
            State = MiniGameState.Idle;
            OnEnd();
            OnMatchEnded?.Invoke();
        }

        IEnumerator ExecuteRound()
        {
            yield return PlayRoundRoutine();

            LastResult = EvaluateResult();
            State = MiniGameState.Resolved;
            OnRoundResolved?.Invoke(LastResult);

            yield return new WaitForSeconds(GetResultDisplayTime());

            State = MiniGameState.WaitingForBet;
            OnReadyForNextRound?.Invoke();
        }

        protected abstract IEnumerator PlayRoundRoutine();
        protected abstract RoundResult EvaluateResult();

        protected virtual void OnBegin() { }
        protected virtual void OnEnd() { }
        protected virtual float GetResultDisplayTime() => 2f;
    }
}
