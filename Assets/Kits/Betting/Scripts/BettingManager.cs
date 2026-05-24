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
        public event Action<int>      OnDuelPrepared;   // npcOpeningBet — se muestra antes de que el jugador actúe
        public event Action           OnRoundFolded;
        public event Action           OnGameAbandoned;
        public event Action           OnGameOver;       // jugador sin fichas
        public event Action           OnNpcBankrupt;    // NPC sin fichas → jugador gana
        public event Action           OnChipsChanged;   // fichas cambiaron (HUD persistente)

        private int  _lastBet;
        private bool _duelPrepared; // true si PrepareDuel ya fijó NpcBet

        void Awake()
        {
            PlayerChips = startingChips;
            NpcChips    = startingChips;
            _lastBet    = minimumBet;
        }

        // ── Preparación del duelo ─────────────────────────────

        /// <summary>
        /// Llamar cuando se abre el panel de apuestas.
        /// El NPC decide su apuesta de apertura para que el jugador la vea antes de actuar.
        /// </summary>
        public void PrepareDuel()
        {
            NpcBet        = NpcBettingAI.DecideBet(NpcChips, _lastBet, minimumBet);
            _duelPrepared = true;
            Debug.Log($"[BettingManager] NPC abre con {NpcBet} fichas.");
            OnDuelPrepared?.Invoke(NpcBet);
        }

        // ── Acciones del jugador ──────────────────────────────

        public void ProcessPlayerAction(BetAction action)
        {
            switch (action)
            {
                case BetAction.Equal:
                    ConfirmBet(Mathf.Min(NpcBet, PlayerChips));
                    break;

                case BetAction.Double:
                    ConfirmBet(Mathf.Min(NpcBet * 2, PlayerChips));
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
                    OnChipsChanged?.Invoke();
                    break;

                case RoundOutcome.Lose:
                    PlayerChips -= EffectiveBet;
                    NpcChips    += EffectiveBet;
                    Debug.Log($"[BettingManager] NPC gana {EffectiveBet} fichas → jugador: {PlayerChips}");
                    OnChipsChanged?.Invoke();
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
            CurrentBet = playerBet;

            // Si PrepareDuel ya fijó la apuesta del NPC, se respeta.
            // Si no (caso sin panel de apuestas), el NPC decide ahora.
            if (!_duelPrepared)
                NpcBet = NpcBettingAI.DecideBet(NpcChips, playerBet, minimumBet);

            _duelPrepared = false;
            EffectiveBet  = Mathf.Min(CurrentBet, NpcBet);

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
            OnChipsChanged?.Invoke();
            OnRoundFolded?.Invoke();
            CheckGameOver();
        }

        private void AbandonGame()
        {
            Debug.Log("[BettingManager] Jugador abandona la partida.");
            OnGameAbandoned?.Invoke();
        }

        private void CheckGameOver()
        {
            if (PlayerChips <= 0)
            {
                Debug.Log("[BettingManager] Game Over — el jugador se quedó sin fichas");
                OnGameOver?.Invoke();
            }
            else if (NpcChips <= 0)
            {
                Debug.Log("[BettingManager] ¡El jugador gana! El NPC se quedó sin fichas");
                OnNpcBankrupt?.Invoke();
            }
        }
    }
}
