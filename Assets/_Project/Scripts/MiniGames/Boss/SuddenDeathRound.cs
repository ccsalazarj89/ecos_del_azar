using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EcosDelAzar.MiniGames.Betting;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// Mecánica de Muerte Súbita para el boss.
    ///
    /// Se activa cuando el jugador tiene al menos (fichas_boss × 2 + 1) fichas.
    /// Ambos lados apuestan todas sus fichas. Se colocan 6 cartas boca abajo,
    /// una de las cuales es un Comodín. Jugador y boss se turnan eligiendo cartas;
    /// quien saque el Comodín pierde todo el pot.
    ///
    /// Integración en Unity:
    ///   1. Añadir al mismo GameObject que MiniGameSession y BettingSystem.
    ///   2. Asignar BettingSystem en el Inspector.
    ///   3. Suscribir la UI a los eventos OnSuddenDeathProposed, OnCardDrawn y
    ///      OnSuddenDeathComplete para mostrar/ocultar paneles.
    ///   4. El jugador elige carta llamando PlayerPickCard(índice) desde botones UI.
    /// </summary>
    public class SuddenDeathRound : MonoBehaviour
    {
        const int TotalCards = 6;

        [Header("Configuración")]
        [Tooltip("El jugador debe tener al menos (fichas_boss × triggerMultiplier + 1) para activar la propuesta.")]
        [SerializeField] int triggerMultiplier = 2;
        [SerializeField] float bossDrawDelay = 1.5f;  // pausa dramática antes de que el boss elija
        [SerializeField] float revealDelay   = 0.8f;  // pausa tras revelar la carta

        [Header("Referencias")]
        [SerializeField] BettingSystem bettingSystem;

        // ------------------------------------------------------------------ eventos

        /// <summary>Dispara cuando se cumplen las condiciones — muestra el panel de propuesta.</summary>
        public event Action OnSuddenDeathProposed;

        /// <summary>
        /// Dispara cada vez que se revela una carta.
        ///   cardIndex  : índice 0-5 de la carta en el tablero
        ///   card       : la carta revelada
        ///   isPlayerTurn : true si la eligió el jugador, false si fue el boss
        /// </summary>
        public event Action<int, Card, bool> OnCardDrawn;

        /// <summary>Dispara al terminar. true = el jugador gana el pot.</summary>
        public event Action<bool> OnSuddenDeathComplete;

        // ------------------------------------------------------------------ estado interno

        bool proposalPending;
        bool suddenDeathActive;

        List<Card> cardPool;
        bool[] drawnFlags;

        int pot;
        int bossCoins;
        bool playerTurn;
        int playerPickedIndex = -1;

        // ------------------------------------------------------------------ lifecycle

        void Start()
        {
            Debug.Log("[SuddenDeathRound] Start() ejecutado.");

            if (bettingSystem == null)
                bettingSystem = GetComponent<BettingSystem>();

            if (bettingSystem == null)
            {
                Debug.LogError("[SuddenDeathRound] BettingSystem no asignado. Desactivando.");
                enabled = false;
                return;
            }

            Debug.Log($"[SuddenDeathRound] Suscrito a BettingSystem. Multiplicador: {triggerMultiplier}x");
            bettingSystem.OnRoundSettled += OnRoundSettled;
        }

        void OnDestroy()
        {
            if (bettingSystem != null)
                bettingSystem.OnRoundSettled -= OnRoundSettled;
        }

        // ------------------------------------------------------------------ detección del trigger

        void OnRoundSettled(RoundOutcome outcome, int winnings)
        {
            Debug.Log($"[SuddenDeathRound] OnRoundSettled: outcome={outcome}, " +
                      $"PlayerCoins={bettingSystem.PlayerCoins}, OpponentCoins={bettingSystem.OpponentCoins}, " +
                      $"activa={suddenDeathActive}, pendiente={proposalPending}");

            if (suddenDeathActive || proposalPending) return;

            bool triggered = bettingSystem.PlayerCoins >= triggerMultiplier * bettingSystem.OpponentCoins + 1;

            if (triggered)
            {
                proposalPending = true;
                Debug.Log($"[SuddenDeathRound] ¡Condición cumplida! Proponiendo muerte súbita.");
                OnSuddenDeathProposed?.Invoke();
                Debug.Log($"[SuddenDeathRound] OnSuddenDeathProposed invocado. Suscriptores: {OnSuddenDeathProposed?.GetInvocationList()?.Length ?? 0}");
            }
            else
            {
                Debug.Log($"[SuddenDeathRound] Condición NO cumplida. Necesita: {triggerMultiplier * bettingSystem.OpponentCoins + 1}");
            }
        }

        // ------------------------------------------------------------------ API pública (botones UI)

        /// <summary>
        /// El jugador acepta la muerte súbita.
        /// Llamar desde el botón "Aceptar" del panel de propuesta.
        /// </summary>
        public void Accept()
        {
            if (!proposalPending) return;
            proposalPending = false;
            StartCoroutine(RunSuddenDeath());
        }

        /// <summary>
        /// El jugador rechaza la muerte súbita. La partida continúa con normalidad.
        /// Llamar desde el botón "Rechazar" del panel de propuesta.
        /// </summary>
        public void Decline()
        {
            if (!proposalPending) return;
            proposalPending = false;
            Debug.Log("[SuddenDeathRound] Muerte súbita rechazada. La partida continúa.");
        }

        /// <summary>
        /// El jugador elige una carta por su índice (0-5).
        /// Llamar desde los botones del tablero de 6 cartas durante el turno del jugador.
        /// Solo tiene efecto cuando suddenDeathActive=true y playerTurn=true.
        /// </summary>
        public void PlayerPickCard(int cardIndex)
        {
            Debug.Log($"[SuddenDeathRound] PlayerPickCard({cardIndex}) — activa={suddenDeathActive}, playerTurn={playerTurn}");
            if (!suddenDeathActive || !playerTurn) return;
            if (cardIndex < 0 || cardIndex >= TotalCards) return;
            if (drawnFlags[cardIndex]) return;

            Debug.Log($"[SuddenDeathRound] Carta {cardIndex} seleccionada por el jugador.");
            playerPickedIndex = cardIndex;
        }

        // ------------------------------------------------------------------ lógica de la ronda

        IEnumerator RunSuddenDeath()
        {
            suddenDeathActive = true;

            // Construir tablero: 5 cartas normales + 1 Comodín en posición aleatoria
            BuildCardPool();

            // All-in: todas las fichas van al pot
            (pot, bossCoins) = bettingSystem.StartSuddenDeath();
            Debug.Log($"[SuddenDeathRound] ¡Muerte súbita! Pot: {pot} (Boss aportó: {bossCoins}, Premio si ganas: {pot - bossCoins + bossCoins * 2}).");

            playerTurn = true;

            for (int turn = 0; turn < TotalCards; turn++)
            {
                if (playerTurn)
                {
                    // ── Turno del jugador ──
                    playerPickedIndex = -1;

                    Debug.Log("[SuddenDeathRound] Turno del jugador — esperando elección...");
                    yield return new WaitUntil(() => playerPickedIndex >= 0);

                    int idx = playerPickedIndex;
                    drawnFlags[idx] = true;
                    Card drawn = cardPool[idx];

                    Debug.Log($"[SuddenDeathRound] Jugador eligió índice {idx}: {drawn}");
                    OnCardDrawn?.Invoke(idx, drawn, true);

                    yield return new WaitForSeconds(revealDelay);

                    if (drawn.Rank == Rank.Joker)
                    {
                        Complete(playerWon: false);
                        yield break;
                    }
                }
                else
                {
                    // ── Turno del boss ──
                    Debug.Log("[SuddenDeathRound] Turno del boss...");
                    yield return new WaitForSeconds(bossDrawDelay);

                    int idx = PickRandomAvailable();
                    drawnFlags[idx] = true;
                    Card drawn = cardPool[idx];

                    Debug.Log($"[SuddenDeathRound] Boss eligió índice {idx}: {drawn}");
                    OnCardDrawn?.Invoke(idx, drawn, false);

                    yield return new WaitForSeconds(revealDelay);

                    if (drawn.Rank == Rank.Joker)
                    {
                        Complete(playerWon: true);
                        yield break;
                    }
                }

                playerTurn = !playerTurn;
            }

            // Salvaguarda: no debería llegar aquí porque el Joker siempre está en el pool
            Debug.LogError("[SuddenDeathRound] Error: el Comodín no fue encontrado en ninguna carta.");
        }

        void Complete(bool playerWon)
        {
            suddenDeathActive = false;
            Debug.Log($"[SuddenDeathRound] Fin. {(playerWon ? "¡El jugador gana!" : "¡El boss gana el pot!")}");
            bettingSystem.ResolveSuddenDeath(playerWon, pot, bossCoins);
            OnSuddenDeathComplete?.Invoke(playerWon);
        }

        // ------------------------------------------------------------------ helpers

        void BuildCardPool()
        {
            cardPool  = new List<Card>(TotalCards);
            drawnFlags = new bool[TotalCards];

            // 5 cartas normales (sin Comodín)
            var deck = new Deck(includeJokers: false);
            for (int i = 0; i < TotalCards - 1; i++)
                cardPool.Add(deck.Draw());

            // Insertar el Comodín en una posición aleatoria
            int jokerPos = UnityEngine.Random.Range(0, TotalCards);
            cardPool.Insert(jokerPos, new Card(Suit.None, Rank.Joker));

            Debug.Log($"[SuddenDeathRound] Pool creado. Comodín en posición {jokerPos} (oculto al jugador).");
        }

        int PickRandomAvailable()
        {
            var available = new List<int>(TotalCards);
            for (int i = 0; i < TotalCards; i++)
                if (!drawnFlags[i]) available.Add(i);

            return available[UnityEngine.Random.Range(0, available.Count)];
        }
    }
}
