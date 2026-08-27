using System;
using UnityEngine;
using EcosDelAzar.AI;

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
    }
}
