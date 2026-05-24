using System;
using UnityEngine;
using EcosDelAzar.AI;
using EcosDelAzar.MiniGames;

namespace EcosDelAzar.Opponent
{
    
    public class AIOpponent : OpponentBase
    {
        [SerializeField] StandardAIBrain brain;

        public override void RequestBet(BetContext context, Action<int> onDecided)
        {
            if (brain == null)
            {
                // Fallback: match the player's bet
                onDecided?.Invoke(Mathf.Min(context.PlayerBet, Coins));
                return;
            }

            int bet = brain.DecideBet(context.PlayerBet, context.MinimumBet, Coins);
            onDecided?.Invoke(bet);
        }

        public override void RequestAction(ActionContext context, Action<CombatAction> onDecided)
        {
            if (brain == null)
            {
                onDecided?.Invoke(CombatAction.Stand);
                return;
            }

            CombatAction action = brain.DecideAction(
                context.EstimatedWinProbability,
                Coins,
                context.CurrentBet
            );
            onDecided?.Invoke(action);
        }
    }
}
