using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EcosDelAzar.MiniGames.Betting;

namespace EcosDelAzar.MiniGames.Boss
{
    public class SuddenDeathRound : MonoBehaviour
    {
        const int TotalCards = 6;

        [Header("Configuración")]
        [Tooltip("El jugador debe tener al menos (fichas_boss × triggerMultiplier + 1) para activar la propuesta.")]
        [SerializeField] int triggerMultiplier = 2;
        [SerializeField] float bossDrawDelay = 1.5f;
        [SerializeField] float revealDelay   = 0.8f;

        [Header("Referencias")]
        [SerializeField] BettingSystem bettingSystem;

        public event Action OnSuddenDeathProposed;

        public event Action<int, Card, bool> OnCardDrawn;

        public event Action<bool> OnSuddenDeathComplete;

        bool proposalPending;
        bool suddenDeathActive;

        List<Card> cardPool;
        bool[] drawnFlags;

        int pot;
        int bossCoins;
        bool playerTurn;
        int playerPickedIndex = -1;

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

        public void Accept()
        {
            if (!proposalPending) return;
            proposalPending = false;
            StartCoroutine(RunSuddenDeath());
        }

        public void Decline()
        {
            if (!proposalPending) return;
            proposalPending = false;
            Debug.Log("[SuddenDeathRound] Muerte súbita rechazada. La partida continúa.");
        }

        public void PlayerPickCard(int cardIndex)
        {
            Debug.Log($"[SuddenDeathRound] PlayerPickCard({cardIndex}) — activa={suddenDeathActive}, playerTurn={playerTurn}");
            if (!suddenDeathActive || !playerTurn) return;
            if (cardIndex < 0 || cardIndex >= TotalCards) return;
            if (drawnFlags[cardIndex]) return;

            Debug.Log($"[SuddenDeathRound] Carta {cardIndex} seleccionada por el jugador.");
            playerPickedIndex = cardIndex;
        }

        IEnumerator RunSuddenDeath()
        {
            suddenDeathActive = true;

            BuildCardPool();

            (pot, bossCoins) = bettingSystem.StartSuddenDeath();
            Debug.Log($"[SuddenDeathRound] ¡Muerte súbita! Pot: {pot} (Boss aportó: {bossCoins}, Premio si ganas: {pot - bossCoins + bossCoins * 2}).");

            playerTurn = true;

            for (int turn = 0; turn < TotalCards; turn++)
            {
                if (playerTurn)
                {
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

            Debug.LogError("[SuddenDeathRound] Error: el Comodín no fue encontrado en ninguna carta.");
        }

        void Complete(bool playerWon)
        {
            suddenDeathActive = false;
            Debug.Log($"[SuddenDeathRound] Fin. {(playerWon ? "¡El jugador gana!" : "¡El boss gana el pot!")}");
            bettingSystem.ResolveSuddenDeath(playerWon, pot, bossCoins);
            OnSuddenDeathComplete?.Invoke(playerWon);
        }

        void BuildCardPool()
        {
            cardPool  = new List<Card>(TotalCards);
            drawnFlags = new bool[TotalCards];

            var deck = new Deck(includeJokers: false);
            for (int i = 0; i < TotalCards - 1; i++)
                cardPool.Add(deck.Draw());

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
