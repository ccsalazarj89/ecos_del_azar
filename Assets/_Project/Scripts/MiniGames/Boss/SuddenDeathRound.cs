using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EcosDelAzar.MiniGames.Betting;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// The boss's all-in challenge. Proposed after a settled round once the player
    /// holds at least triggerMultiplier × the boss's stack. Accept: both stacks go
    /// into the pot and player and boss alternate drawing from 7 face-down cards
    /// (6 + one joker); whoever draws the joker loses. The 6 safe cards can all
    /// come out — then the row is reshuffled and the duel goes on until the joker
    /// appears. Decline: the boss keeps a cut of the player's coins and may ask
    /// again after a later round.
    /// </summary>
    public class SuddenDeathRound : MonoBehaviour
    {
        public const int SafeCards = 6;
        public const int TotalCards = SafeCards + 1;

        [Tooltip("Player coins needed relative to the boss's stack: coins >= multiplier × bossCoins + 1.")]
        [SerializeField] int triggerMultiplier = 2;
        [Tooltip("Share of the player's coins the boss takes when the challenge is declined.")]
        [SerializeField, Range(0f, 0.5f)] float declineFee = 0.10f;
        [SerializeField] float bossDrawDelay = 1.5f;
        [SerializeField] float revealDelay = 0.8f;
        [SerializeField] float reshuffleDelay = 1.2f;

        [SerializeField] BettingSystem bettingSystem;

        public event Action OnSuddenDeathProposed;
        /// <summary>Card revealed at index, and whether the player drew it.</summary>
        public event Action<int, Card, bool> OnCardDrawn;
        /// <summary>All safe cards are out: the row is dealt again.</summary>
        public event Action<int> OnReshuffled;
        public event Action<int> OnDeclined;
        public event Action<bool> OnSuddenDeathComplete;

        public int DeclineFeeFor(int playerCoins) => Mathf.RoundToInt(playerCoins * declineFee);

        bool proposalPending;
        bool active;
        List<Card> cardPool;
        bool[] drawn;
        int pot;
        int bossCoins;
        bool playerTurn;
        int playerPick = -1;
        int round;

        void Start()
        {
            if (bettingSystem == null) bettingSystem = GetComponent<BettingSystem>();
            if (bettingSystem == null) { enabled = false; return; }
            bettingSystem.OnRoundSettled += OnRoundSettled;
        }

        void OnDestroy()
        {
            if (bettingSystem != null) bettingSystem.OnRoundSettled -= OnRoundSettled;
        }

        void OnRoundSettled(RoundOutcome outcome, int winnings)
        {
            if (active) return;
            if (bettingSystem.PlayerCoins < triggerMultiplier * bettingSystem.OpponentCoins + 1) { proposalPending = false; return; }

            proposalPending = true;
            OnSuddenDeathProposed?.Invoke();
        }

        public void Accept()
        {
            if (!proposalPending) return;
            proposalPending = false;
            StartCoroutine(Run());
        }

        public void Decline()
        {
            if (!proposalPending) return;
            proposalPending = false;

            int fee = bettingSystem.TakeDeclineFee(declineFee);
            OnDeclined?.Invoke(fee);
        }

        public void PlayerPickCard(int index)
        {
            if (!active || !playerTurn) return;
            if (index < 0 || index >= TotalCards || drawn[index]) return;
            playerPick = index;
        }

        IEnumerator Run()
        {
            active = true;
            round = 0;
            (pot, bossCoins) = bettingSystem.StartSuddenDeath();
            playerTurn = true;

            while (true)
            {
                DealRow();
                round++;
                if (round > 1)
                {
                    OnReshuffled?.Invoke(round);
                    yield return new WaitForSeconds(reshuffleDelay);
                }

                // Only SafeCards draws per row: the joker can survive a whole row (1 in 7), which is when the reshuffle kicks in.
                for (int turn = 0; turn < SafeCards; turn++)
                {
                    int index;
                    if (playerTurn)
                    {
                        playerPick = -1;
                        yield return new WaitUntil(() => playerPick >= 0);
                        index = playerPick;
                    }
                    else
                    {
                        yield return new WaitForSeconds(bossDrawDelay);
                        index = PickRandomAvailable();
                    }

                    drawn[index] = true;
                    var card = cardPool[index];
                    OnCardDrawn?.Invoke(index, card, playerTurn);
                    yield return new WaitForSeconds(revealDelay);

                    if (card.Rank == Rank.Joker)
                    {
                        Complete(playerWon: !playerTurn);
                        yield break;
                    }

                    playerTurn = !playerTurn;
                }
            }
        }

        void Complete(bool playerWon)
        {
            active = false;
            bettingSystem.ResolveSuddenDeath(playerWon, pot, bossCoins);
            OnSuddenDeathComplete?.Invoke(playerWon);
        }

        void DealRow()
        {
            cardPool = new List<Card>(TotalCards);
            drawn = new bool[TotalCards];

            var deck = new Deck(includeJokers: false);
            for (int i = 0; i < SafeCards; i++) cardPool.Add(deck.Draw());
            cardPool.Insert(UnityEngine.Random.Range(0, TotalCards), new Card(Suit.None, Rank.Joker));
        }

        int PickRandomAvailable()
        {
            var available = new List<int>(TotalCards);
            for (int i = 0; i < TotalCards; i++) if (!drawn[i]) available.Add(i);
            return available[UnityEngine.Random.Range(0, available.Count)];
        }
    }
}
