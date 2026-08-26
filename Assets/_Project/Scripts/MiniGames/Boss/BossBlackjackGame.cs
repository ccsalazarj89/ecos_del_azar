using System.Collections.Generic;
using UnityEngine;
using EcosDelAzar.MiniGames.Blackjack;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// Blackjack against the boss. Same rules as the floor game; adds the forced
    /// win the player can buy with oxygen after two straight losses.
    /// </summary>
    public class BossBlackjackGame : BlackjackGame, IBossGame
    {
        public IReadOnlyList<Card> PlayerRoundCards => PlayerHand.Cards;
        public bool PlayerBusted => PlayerHand.IsBust;
        public bool IsForceWinQueued { get; private set; }

        public void QueueForceWin()
        {
            IsForceWinQueued = true;
            Debug.Log("[BossBlackjackGame] Forced win queued — next round resolves as Win.");
        }

        protected override RoundResult EvaluateResult()
        {
            var result = base.EvaluateResult();
            if (!IsForceWinQueued) return result;

            IsForceWinQueued = false;
            return new RoundResult(
                outcome: RoundOutcome.Win,
                playerValue: result.PlayerValue,
                opponentValue: result.OpponentValue,
                description: $"[Victoria Forzada] {result.Description}"
            );
        }
    }
}
