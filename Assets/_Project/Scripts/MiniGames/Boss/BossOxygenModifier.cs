using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.MiniGames.HighCard;

namespace EcosDelAzar.MiniGames.Boss
{
    [RequireComponent(typeof(MiniGameSession))]
    public class BossOxygenModifier : MonoBehaviour
    {
        [Header("Palo del Boss")]
        [Tooltip("None = aleatorio al inicio de cada sesión.")]
        [SerializeField] Suit forcedBossSuit = Suit.None;

        [Header("Restaurar oxígeno al ganar con palo del boss (% del máximo)")]
        [SerializeField, Range(0f, 1f)] float jackRestorePercent  = 0.30f;
        [SerializeField, Range(0f, 1f)] float queenRestorePercent = 0.50f;
        [SerializeField, Range(0f, 1f)] float kingRestorePercent  = 0.70f;
        [SerializeField, Range(0f, 1f)] float aceRestorePercent   = 1.00f;

        [Header("Penalización al perder con palo del boss (% del máximo)")]
        [SerializeField, Range(0f, 1f)] float losePenaltyPercent = 0.20f;

        [Header("Comodín (% del máximo que se drena)")]
        [SerializeField, Range(0f, 1f)] float jokerDepletePercent = 0.50f;

        [Header("Victoria Forzada")]
        [Tooltip("% del oxígeno máximo que cuesta activar la victoria forzada.")]
        [SerializeField, Range(0f, 1f)] float forceWinOxygenCost = 0.30f;

        public Suit AssignedSuit { get; private set; }

        public bool CanAffordForceWin
        {
            get
            {
                var tank = GameManager.Instance?.OxygenTank;
                return tank != null && tank.Current >= tank.Max * forceWinOxygenCost;
            }
        }

        public int ConsecutiveLosses { get; private set; }

        public bool IsForceWinAvailable => ConsecutiveLosses >= 2 && CanAffordForceWin;

        public event System.Action OnForceWinAvailabilityChanged;

        MiniGameSession session;
        HighCardGame highCardGame;
        BossHighCardGame bossGame;

        void Awake()
        {
            session = GetComponent<MiniGameSession>();

            AssignBossSuit();
        }

        void Start()
        {
            if (session == null || session.Game == null)
            {
                Debug.LogError("[BossOxygenModifier] MiniGameSession o Game no encontrado. Se desactiva.");
                enabled = false;
                return;
            }

            highCardGame = session.Game as HighCardGame;
            if (highCardGame == null)
            {
                Debug.LogError("[BossOxygenModifier] El minijuego no es HighCardGame. Se desactiva.");
                enabled = false;
                return;
            }

            bossGame = highCardGame as BossHighCardGame;
            if (bossGame == null)
                Debug.LogWarning("[BossOxygenModifier] BossHighCardGame no encontrado — TryActivateForceWin() no funcionará.");

            session.Game.OnRoundResolved += HandleRoundResolved;
        }

        void OnDestroy()
        {
            if (session?.Game != null)
                session.Game.OnRoundResolved -= HandleRoundResolved;
        }

        public bool TryActivateForceWin()
        {
            if (bossGame == null)
            {
                Debug.LogWarning("[BossOxygenModifier] Se necesita BossHighCardGame para usar la victoria forzada.");
                return false;
            }

            if (bossGame.IsForceWinQueued)
            {
                Debug.Log("[BossOxygenModifier] Ya hay una victoria forzada en cola.");
                return false;
            }

            OxygenTank tank = GameManager.Instance?.OxygenTank;
            if (tank == null) return false;

            float cost = tank.Max * forceWinOxygenCost;
            if (tank.Current < cost)
            {
                Debug.Log($"[BossOxygenModifier] Oxígeno insuficiente. Necesitas {cost:F1}, tienes {tank.Current:F1}.");
                return false;
            }

            tank.Deplete(cost);
            bossGame.QueueForceWin();
            ConsecutiveLosses = 0;
            OnForceWinAvailabilityChanged?.Invoke();
            Debug.Log($"[BossOxygenModifier] Victoria forzada activada. Coste: {cost:F1} oxígeno ({forceWinOxygenCost * 100f:F0}%).");
            return true;
        }

        [ContextMenu("DEBUG: Simular victoria con K del palo del boss")]
        void DebugSimulateKingWin()
        {
            var tank = GameManager.Instance?.OxygenTank;
            if (tank == null) { Debug.LogWarning("[DEBUG] OxygenTank no encontrado."); return; }
            float amount = tank.Max * kingRestorePercent;
            tank.Restore(amount);
            Debug.Log($"[DEBUG] Simulado K de {AssignedSuit} — +{amount:F1} oxígeno.");
        }

        [ContextMenu("DEBUG: Vaciar oxígeno (game over)")]
        void DebugDepleteAllOxygen()
        {
            var tank = GameManager.Instance?.OxygenTank;
            if (tank == null) { Debug.LogWarning("[DEBUG] OxygenTank no encontrado."); return; }
            tank.Deplete(tank.Max);
            Debug.Log("[DEBUG] Oxígeno vaciado — debería dispararse game over.");
        }

        void AssignBossSuit()
        {
            if (forcedBossSuit != Suit.None)
            {
                AssignedSuit = forcedBossSuit;
            }
            else
            {
                Suit[] validSuits = { Suit.Hearts, Suit.Diamonds, Suit.Spades, Suit.Clubs };
                AssignedSuit = validSuits[Random.Range(0, validSuits.Length)];
            }
            Debug.Log($"[BossOxygenModifier] Palo del boss para esta sesión: {AssignedSuit}");
        }

        void HandleRoundResolved(RoundResult result)
        {
            OxygenTank tank = GameManager.Instance?.OxygenTank;
            if (tank == null) return;

            Card playerCard = highCardGame.PlayerCard;

            bool prevAvailable = IsForceWinAvailable;
            if (result.Outcome == RoundOutcome.Lose)
                ConsecutiveLosses++;
            else
                ConsecutiveLosses = 0;

            var debugTank = GameManager.Instance?.OxygenTank;
            Debug.Log($"[BossOxygenModifier] Ronda resuelta: {result.Outcome} | " +
                      $"Derrotas consecutivas: {ConsecutiveLosses} | " +
                      $"OxygenTank: {(debugTank != null ? $"{debugTank.Current:F0}/{debugTank.Max:F0}" : "NULL")} | " +
                      $"ForceWin disponible: {IsForceWinAvailable}");

            if (prevAvailable != IsForceWinAvailable)
                OnForceWinAvailabilityChanged?.Invoke();

            if (playerCard.Rank == Rank.Joker)
            {
                float jokerAmount = tank.Max * jokerDepletePercent;
                tank.Deplete(jokerAmount);
                Debug.Log($"[BossOxygenModifier] ¡Comodín! -{jokerAmount:F1} oxígeno.");
                return;
            }

            if (result.Outcome == RoundOutcome.Lose && playerCard.Suit == AssignedSuit)
            {
                float penalty = tank.Max * losePenaltyPercent;
                tank.Deplete(penalty);
                Debug.Log($"[BossOxygenModifier] Derrota con {AssignedSuit} (palo del boss). -{penalty:F1} oxígeno extra.");
                return;
            }

            if (result.Outcome == RoundOutcome.Win && playerCard.Suit == AssignedSuit)
            {
                float restorePercent = playerCard.Rank switch
                {
                    Rank.Jack  => jackRestorePercent,
                    Rank.Queen => queenRestorePercent,
                    Rank.King  => kingRestorePercent,
                    Rank.Ace   => aceRestorePercent,
                    _          => 0f
                };

                if (restorePercent > 0f)
                {
                    float restoreAmount = tank.Max * restorePercent;
                    tank.Restore(restoreAmount);
                    Debug.Log($"[BossOxygenModifier] {playerCard.Rank} de {AssignedSuit} — +{restoreAmount:F1} oxígeno ({restorePercent * 100f:F0}%).");
                }
            }
        }
    }
}
