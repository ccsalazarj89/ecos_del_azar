using System;
using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.MiniGames.Betting;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.MiniGames
{
    /// <summary>
    /// Orchestrates one seating at a table: betting + minigame rounds until the
    /// player folds or someone goes broke. Restores and persists the table's
    /// per-run state (opponent bankroll, escalated minimum) and awards a house
    /// chip when the opponent is bankrupted.
    /// </summary>
    public class MiniGameSession : MonoBehaviour
    {
        [SerializeField] MiniGameBase miniGame;
        [SerializeField] MiniGameConfig config;
        [SerializeField] BettingSystem bettingSystem;

        public MiniGameBase Game => miniGame;
        public BettingSystem Betting => bettingSystem;
        public MiniGameConfig Config => config;
        public int RoundsPlayed { get; private set; }

        public event Action OnSessionStarted;
        public event Action OnSessionEnded;

        string TableId => ElevatorSceneLoader.CurrentTableId;
        bool HasTable => !string.IsNullOrEmpty(TableId);

        void Start()
        {
            BeginSession();
        }

        void OnEnable()
        {
            if (miniGame != null)
                miniGame.OnRoundResolved += OnRoundResolved;

            if (bettingSystem != null)
                bettingSystem.OnGameOver += OnGameOver;
        }

        void OnDisable()
        {
            if (miniGame != null)
                miniGame.OnRoundResolved -= OnRoundResolved;

            if (bettingSystem != null)
                bettingSystem.OnGameOver -= OnGameOver;
        }

        public void StartRound(int playerBet)
        {
            if (miniGame.State != MiniGameState.WaitingForBet) return;

            bettingSystem.PlaceBets(playerBet);
            miniGame.PlayRound();
        }

        public void RespondToProposal(BetResponse response)
        {
            if (response == BetResponse.Fold)
            {
                bettingSystem.PlayerFolds();
                Leave();
                return;
            }

            bool doubled = response == BetResponse.Double;
            int nextBet = doubled ? bettingSystem.LastBet * 2 : bettingSystem.NpcProposedBet;

            bettingSystem.PlaceBets(nextBet, doubled);
            miniGame.PlayRound();
        }

        public void Leave()
        {
            if (GameManager.Instance?.OxygenTank != null)
                GameManager.Instance.OxygenTank.IsActiveDrain = false;

            PersistTableState();
            miniGame.End();
            OnSessionEnded?.Invoke();
            ElevatorSceneLoader.ReturnToHub();
        }

        void BeginSession()
        {
            RoundsPlayed = 0;

            int opponentCoins = HasTable ? TableState.GetOpponentCoins(TableId) : -1;
            int minimumBet = HasTable ? TableState.GetMinimumBet(TableId) : 0;
            bettingSystem.Initialize(opponentCoins, minimumBet);

            if (bettingSystem.IsPlayerBroke)
            {
                OnSessionStarted?.Invoke();
                bettingSystem.ForceGameOver(false);
                return;
            }

            if (GameManager.Instance?.OxygenTank != null)
                GameManager.Instance.OxygenTank.IsActiveDrain = true;

            miniGame.Begin();
            OnSessionStarted?.Invoke();
        }

        void OnRoundResolved(RoundResult result)
        {
            bettingSystem.ResolveResult(result.Outcome);
            RoundsPlayed++;
            // No round cap: the match runs until the player folds or someone goes broke.
        }

        void OnGameOver(bool playerWon)
        {
            PersistTableState();

            if (playerWon && HasTable && !TableState.IsBeaten(TableId))
            {
                TableState.MarkBeaten(TableId);
                if (ElevatorSceneLoader.CurrentTableAwardsChip)
                    HouseChips.Award(ElevatorSceneLoader.CurrentTableFloor);
            }
            // The betting UI shows the game-over panel; leaving is the player's click.
        }

        // The table remembers the opponent's stack and its last proposal, so folding
        // and re-entering cannot reset the stakes (see docs: "Design note: folding").
        void PersistTableState()
        {
            if (!HasTable) return;
            int minimum = Mathf.Min(bettingSystem.NpcProposedBet, bettingSystem.OpponentCoins);
            TableState.Save(TableId, bettingSystem.OpponentCoins, minimum);
        }
    }
}
