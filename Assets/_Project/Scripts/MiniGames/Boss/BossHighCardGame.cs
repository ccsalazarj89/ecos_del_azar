using System.Collections.Generic;
using UnityEngine;
using EcosDelAzar.MiniGames.HighCard;

namespace EcosDelAzar.MiniGames.Boss
{
    public class BossHighCardGame : HighCardGame, IBossGame
    {
        public IReadOnlyList<Card> PlayerRoundCards => new[] { PlayerCard };
        public bool PlayerBusted => false;
        public bool IsForceWinQueued { get; private set; }

        public void QueueForceWin()
        {
            IsForceWinQueued = true;
            Debug.Log("[BossHighCardGame] Forced win queued — next round resolves as Win.");
        }

        protected override RoundResult EvaluateResult()
        {
            if (!IsForceWinQueued)
                return base.EvaluateResult();

            IsForceWinQueued = false;

            return new RoundResult(
                outcome: RoundOutcome.Win,
                playerValue: PlayerCard.Value,
                opponentValue: 0,
                description: $"[Victoria Forzada] {PlayerCard}"
            );
        }
    }
}
