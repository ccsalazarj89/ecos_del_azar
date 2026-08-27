using System.Collections.Generic;
using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// The boss's house rules: a suit is assigned to the boss for the session and
    /// the player's cards of that suit move the oxygen tank on each round. Also
    /// owns the "forced win" the player can buy with oxygen after two straight
    /// losses. Works with any minigame that implements IBossGame (today: BossBlackjackGame).
    /// </summary>
    [RequireComponent(typeof(MiniGameSession))]
    public class BossOxygenModifier : MonoBehaviour
    {
        [Header("Boss suit")]
        [Tooltip("None = random at the start of each session.")]
        [SerializeField] Suit forcedBossSuit = Suit.None;

        [Header("Restore on a win holding the boss suit (% of max, best card counts)")]
        [SerializeField, Range(0f, 1f)] float jackRestorePercent  = 0.30f;
        [SerializeField, Range(0f, 1f)] float queenRestorePercent = 0.50f;
        [SerializeField, Range(0f, 1f)] float kingRestorePercent  = 0.70f;
        [SerializeField, Range(0f, 1f)] float aceRestorePercent   = 1.00f;

        [Header("Penalties (% of max)")]
        [Tooltip("Losing while holding a card of the boss suit.")]
        [SerializeField, Range(0f, 1f)] float losePenaltyPercent = 0.20f;
        [Tooltip("Busting in Blackjack: the player's own call, so it costs air.")]
        [SerializeField, Range(0f, 1f)] float bustPenaltyPercent = 0.25f;

        [Header("Forced win")]
        [Tooltip("% of max oxygen it costs to activate the forced win.")]
        [SerializeField, Range(0f, 1f)] float forceWinOxygenCost = 0.30f;

        public Suit AssignedSuit { get; private set; }
        public int ConsecutiveLosses { get; private set; }

        public bool CanAffordForceWin
        {
            get
            {
                var tank = GameManager.Instance?.OxygenTank;
                return tank != null && tank.Current >= tank.Max * forceWinOxygenCost;
            }
        }

        public bool IsForceWinAvailable => ConsecutiveLosses >= 2 && CanAffordForceWin;

        public event System.Action OnForceWinAvailabilityChanged;

        MiniGameSession session;
        IBossGame bossGame;

        void Awake()
        {
            session = GetComponent<MiniGameSession>();
            AssignBossSuit();
        }

        void Start()
        {
            bossGame = session?.Game as IBossGame;
            if (bossGame == null)
            {
                Debug.LogError("[BossOxygenModifier] The session's game must implement IBossGame (BossBlackjackGame). Disabled.");
                enabled = false;
                return;
            }

            session.Game.OnRoundResolved += HandleRoundResolved;
        }

        void OnDestroy()
        {
            if (session?.Game != null)
                session.Game.OnRoundResolved -= HandleRoundResolved;
        }

        public bool TryActivateForceWin()
        {
            if (bossGame == null || bossGame.IsForceWinQueued) return false;

            var tank = GameManager.Instance?.OxygenTank;
            if (tank == null) return false;

            float cost = tank.Max * forceWinOxygenCost;
            if (tank.Current < cost) return false;

            tank.Deplete(cost);
            bossGame.QueueForceWin();
            ConsecutiveLosses = 0;
            OnForceWinAvailabilityChanged?.Invoke();
            return true;
        }

        void AssignBossSuit()
        {
            if (forcedBossSuit != Suit.None)
            {
                AssignedSuit = forcedBossSuit;
                return;
            }

            Suit[] validSuits = { Suit.Hearts, Suit.Diamonds, Suit.Spades, Suit.Clubs };
            AssignedSuit = validSuits[Random.Range(0, validSuits.Length)];
        }

        void HandleRoundResolved(RoundResult result)
        {
            var tank = GameManager.Instance?.OxygenTank;
            if (tank == null) return;

            bool prevAvailable = IsForceWinAvailable;
            ConsecutiveLosses = result.Outcome == RoundOutcome.Lose ? ConsecutiveLosses + 1 : 0;
            if (prevAvailable != IsForceWinAvailable)
                OnForceWinAvailabilityChanged?.Invoke();

            var cards = bossGame.PlayerRoundCards;

            if (result.Outcome == RoundOutcome.Lose)
            {
                if (bossGame.PlayerBusted)
                    tank.Deplete(tank.Max * bustPenaltyPercent);
                else if (HoldsBossSuit(cards))
                    tank.Deplete(tank.Max * losePenaltyPercent);
                return;
            }

            if (result.Outcome == RoundOutcome.Win)
            {
                float restore = BestRestorePercent(cards);
                if (restore > 0f) tank.Restore(tank.Max * restore);
            }
        }

        bool HoldsBossSuit(IReadOnlyList<Card> cards)
        {
            foreach (var c in cards)
                if (c != null && c.Suit == AssignedSuit) return true;
            return false;
        }

        float BestRestorePercent(IReadOnlyList<Card> cards)
        {
            float best = 0f;
            foreach (var c in cards)
            {
                if (c == null || c.Suit != AssignedSuit) continue;
                float p = c.Rank switch
                {
                    Rank.Jack  => jackRestorePercent,
                    Rank.Queen => queenRestorePercent,
                    Rank.King  => kingRestorePercent,
                    Rank.Ace   => aceRestorePercent,
                    _          => 0f
                };
                if (p > best) best = p;
            }
            return best;
        }
    }
}
