using System.Collections.Generic;
using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// The boss's house rule: the Director owns a suit for the whole run, and the
    /// figures of that suit in the player's hand move the oxygen tank — they save
    /// the player on a win and cost air on a loss (symmetric on purpose: rewarding
    /// only J/Q/K/A while punishing any card of the suit read as an arbitrary tax). Works with any minigame that implements IBossGame (today: BossBlackjackGame).
    /// </summary>
    [RequireComponent(typeof(MiniGameSession))]
    public class BossOxygenModifier : MonoBehaviour
    {
        const string SuitKey = "boss.suit";

        [Header("Boss suit")]
        [Tooltip("None = random at the start of each session.")]
        [SerializeField] Suit forcedBossSuit = Suit.None;

        [Header("Boss-suit figures: restore on a win (% of max, best card counts)")]
        [SerializeField, Range(0f, 1f)] float jackRestorePercent  = 0.30f;
        [SerializeField, Range(0f, 1f)] float queenRestorePercent = 0.50f;
        [SerializeField, Range(0f, 1f)] float kingRestorePercent  = 0.70f;
        [SerializeField, Range(0f, 1f)] float aceRestorePercent   = 1.00f;

        [Header("Penalties (% of max)")]
        [Tooltip("Losing while holding a FIGURE or ace of the boss suit.")]
        [SerializeField, Range(0f, 1f)] float losePenaltyPercent = 0.20f;
        [Tooltip("Busting in Blackjack: the player's own call, so it costs air.")]
        [SerializeField, Range(0f, 1f)] float bustPenaltyPercent = 0.25f;

        public Suit AssignedSuit { get; private set; }

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

        void AssignBossSuit()
        {
            if (forcedBossSuit != Suit.None)
            {
                AssignedSuit = forcedBossSuit;
                return;
            }

            // The suit is the Director's trait, not a per-seating roll: drawing it again
            // on every re-entry would let the player stand up and reroll a bad suit.
            int saved = RunPrefs.GetInt(SuitKey, -1);
            if (saved >= 0 && saved <= (int)Suit.Clubs)
            {
                AssignedSuit = (Suit)saved;
                return;
            }

            Suit[] validSuits = { Suit.Hearts, Suit.Diamonds, Suit.Spades, Suit.Clubs };
            AssignedSuit = validSuits[Random.Range(0, validSuits.Length)];
            RunPrefs.SetInt(SuitKey, (int)AssignedSuit);
            RunPrefs.Save();
        }

        void HandleRoundResolved(RoundResult result)
        {
            var tank = GameManager.Instance?.OxygenTank;
            if (tank == null) return;

            var cards = bossGame.PlayerRoundCards;

            if (result.Outcome == RoundOutcome.Lose)
            {
                if (bossGame.PlayerBusted)
                {
                    tank.Deplete(tank.Max * bustPenaltyPercent);
                    tank.Report(-tank.Max * bustPenaltyPercent, "Te pasaste de 21");
                }
                else if (BestRestorePercent(cards) > 0f)
                {
                    tank.Deplete(tank.Max * losePenaltyPercent);
                    tank.Report(-tank.Max * losePenaltyPercent, $"Caíste con una figura de {SuitName(AssignedSuit)}");
                }
                return;
            }

            if (result.Outcome == RoundOutcome.Win)
            {
                float restore = BestRestorePercent(cards);
                if (restore > 0f)
                {
                    tank.Restore(tank.Max * restore);
                    tank.Report(tank.Max * restore, $"Venciste con una figura de {SuitName(AssignedSuit)}");
                }
            }
        }

        static string SuitName(Suit suit) => suit switch
        {
            Suit.Hearts => "corazones",
            Suit.Diamonds => "diamantes",
            Suit.Spades => "picas",
            Suit.Clubs => "tréboles",
            _ => "?"
        };

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
