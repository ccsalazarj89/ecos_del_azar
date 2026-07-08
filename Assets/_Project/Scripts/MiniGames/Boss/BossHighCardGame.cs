using UnityEngine;
using EcosDelAzar.MiniGames.HighCard;

namespace EcosDelAzar.MiniGames.Boss
{
    public class BossHighCardGame : HighCardGame
    {
        public bool IsForceWinQueued { get; private set; }

        public void QueueForceWin()
        {
            IsForceWinQueued = true;
            Debug.Log("[BossHighCardGame] Victoria forzada en cola — la próxima ronda será Win.");
        }

        protected override RoundResult EvaluateResult()
        {
            if (!IsForceWinQueued)
                return base.EvaluateResult();

            IsForceWinQueued = false;
            Debug.Log($"[BossHighCardGame] Victoria forzada aplicada. Carta: {PlayerCard}");

            return new RoundResult(
                outcome: RoundOutcome.Win,
                playerValue: PlayerCard.Value,
                opponentValue: 0,
                description: $"[Victoria Forzada] {PlayerCard}"
            );
        }
    }
}
