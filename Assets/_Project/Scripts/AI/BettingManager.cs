using System;
using UnityEngine;

namespace EcosDelAzar.Betting
{
    public enum BetAction    { Equal, Double, AllIn, FoldRound, AbandonGame }
    public enum RoundOutcome { Win, Lose, Draw }

    /// <summary>
    /// Gestiona el estado de las fichas y la lógica de apuestas.
    /// Genérico — no depende de ningún minijuego concreto.
    /// Cualquier minijuego llama a ResolveResult(outcome) al terminar.
    /// </summary>
    public class BettingManager : MonoBehaviour
    {
        [Header("Configuración")]
        public int startingChips = 1000;
        public int minimumBet    = 10;

        // ── Estado público ────────────────────────────────────
        public int PlayerChips  { get; private set; }
        public int NpcChips     { get; private set; }
        public int CurrentBet   { get; private set; }
        public int NpcBet       { get; private set; }
        public int EffectiveBet { get; private set; }

        // ── Eventos ───────────────────────────────────────────
        public event Action<int, int> OnBetConfirmed;   // playerBet, npcBet
        public event Action           OnRoundFolded;
        public event Action           OnGameAbandoned;
        public event Action           OnGameOver;

        private int _lastBet;

        void Awake()
        {
            PlayerChips = startingChips;
            NpcChips    = startingChips;
            _lastBet    = minimumBet;
        }

        // ── Acciones del jugador ──────────────────────────────

        public void ProcessPlayerAction(BetAction action)
        {
            switch (action)
            {
                case BetAction.Equal:
                    ConfirmBet(minimumBet);
                    break;

                case BetAction.Double:
                    ConfirmBet(Mathf.Min(_lastBet * 2, PlayerChips));
                    break;

                case BetAction.AllIn:
                    ConfirmBet(PlayerChips);
                    break;

                case BetAction.FoldRound:
                    FoldRound();
                    break;

                case BetAction.AbandonGame:
                    AbandonGame();
                    break;
            }
        }

        /// <summary>
        /// Cualquier minijuego llama este método al terminar la ronda.
        /// Win/Lose/Draw desde la perspectiva del jugador humano.
        /// </summary>
        public void ResolveResult(RoundOutcome outcome)
        {
            switch (outcome)
            {
                case RoundOutcome.Win:
                    PlayerChips += EffectiveBet;
                    NpcChips    -= EffectiveBet;
                    Debug.Log($"[BettingManager] Jugador gana {EffectiveBet} fichas → {PlayerChips}");
                    break;

                case RoundOutcome.Lose:
                    PlayerChips -= EffectiveBet;
                    NpcChips    += EffectiveBet;
                    Debug.Log($"[BettingManager] NPC gana {EffectiveBet} fichas → jugador: {PlayerChips}");
                    break;

                case RoundOutcome.Draw:
                    Debug.Log("[BettingManager] Empate — se devuelven las fichas");
                    break;
            }

            _lastBet = CurrentBet;
            CheckGameOver();
        }

        // ── Privados ──────────────────────────────────────────

        private void ConfirmBet(int playerBet)
        {
            CurrentBet   = playerBet;
            NpcBet       = NpcBettingAI.DecideBet(NpcChips, playerBet, minimumBet);
            EffectiveBet = Mathf.Min(CurrentBet, NpcBet);

            Debug.Log($"[BettingManager] Jugador apuesta {CurrentBet} | NPC apuesta {NpcBet} | Efectiva: {EffectiveBet}");
            OnBetConfirmed?.Invoke(CurrentBet, NpcBet);
        }

        private void FoldRound()
        {
            int penalty = Mathf.Min(minimumBet, PlayerChips);
            PlayerChips -= penalty;
            NpcChips    += penalty;
            _lastBet     = minimumBet;

            Debug.Log($"[BettingManager] Jugador se retira — pierde {penalty} fichas → {PlayerChips}");
            OnRoundFolded?.Invoke();
            CheckGameOver();
        }

        private void AbandonGame()
        {
            int penalty = Mathf.RoundToInt(PlayerChips * 0.1f);
            PlayerChips -= penalty;

            Debug.Log($"[BettingManager] Jugador abandona — penalización {penalty} fichas → {PlayerChips}");
            OnGameAbandoned?.Invoke();
        }

        private void CheckGameOver()
        {
            if (PlayerChips <= 0)
            {
                Debug.Log("[BettingManager] Game Over — el jugador se quedó sin fichas");
                OnGameOver?.Invoke();
            }
        }
    }
}
