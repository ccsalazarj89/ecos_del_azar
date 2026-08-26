using System;
using System.Collections;
using UnityEngine;

namespace EcosDelAzar.MiniGames.Dice
{
    public class DiceGame : MiniGameBase
    {
        public override string PlayingStatusText => "Lanzando dados...";

        [SerializeField] int faces = 6;
        [SerializeField] float rollDuration = 1.2f;
        [SerializeField] float frameInterval = 0.05f;

        public int PlayerRoll { get; private set; }
        public int OpponentRoll { get; private set; }

        public event Action<int, int> OnRolling;
        public event Action<int, int> OnRollFinished;

        protected override IEnumerator PlayRoundRoutine()
        {
            float elapsed = 0f;

            while (elapsed < rollDuration)
            {
                int tempPlayer = UnityEngine.Random.Range(1, faces + 1);
                int tempOpponent = UnityEngine.Random.Range(1, faces + 1);

                OnRolling?.Invoke(tempPlayer, tempOpponent);

                yield return new WaitForSeconds(frameInterval);
                elapsed += frameInterval;
            }

            PlayerRoll = UnityEngine.Random.Range(1, faces + 1);
            OpponentRoll = UnityEngine.Random.Range(1, faces + 1);
            OnRollFinished?.Invoke(PlayerRoll, OpponentRoll);
        }

        protected override RoundResult EvaluateResult()
        {
            RoundOutcome outcome = PlayerRoll > OpponentRoll ? RoundOutcome.Win
                                 : OpponentRoll > PlayerRoll ? RoundOutcome.Lose
                                 : RoundOutcome.Draw;

            return new RoundResult(outcome, PlayerRoll, OpponentRoll);
        }
    }
}
