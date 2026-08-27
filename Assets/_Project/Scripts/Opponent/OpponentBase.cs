using System;
using UnityEngine;

namespace EcosDelAzar.Opponent
{
    /// <summary>
    /// Abstract MonoBehaviour base for opponents.
    /// Exists so Unity can serialize opponent references in the Inspector
    /// (you can't serialize interfaces, but you can serialize MonoBehaviour subclasses).
    /// 
    /// Subclass this for each opponent type:
    ///   - AIOpponent (responds instantly)
    ///   - LocalPlayerOpponent (waits for second controller input)
    ///   - NetworkOpponent (waits for network message)
    /// </summary>
    public abstract class OpponentBase : MonoBehaviour, IOpponent
    {
        [SerializeField] protected int startingCoins = 1000;

        public int Coins { get; set; }

        protected virtual void Awake()
        {
            Coins = startingCoins;
        }

        public abstract void RequestBet(BetContext context, Action<int> onDecided);

        public virtual void ResetSession()
        {
            Coins = startingCoins;
        }
    }
}
