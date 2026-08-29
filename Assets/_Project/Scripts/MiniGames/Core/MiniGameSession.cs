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
        [SerializeField] BettingSystem bettingSystem;

        public MiniGameBase Game => miniGame;
        public BettingSystem Betting => bettingSystem;

        public event Action OnSessionStarted;

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

        public void StartRound(int playerBet, RoundStance stance = RoundStance.Stand)
        {
            if (miniGame.State != MiniGameState.WaitingForBet) return;

            bettingSystem.PlaceBets(playerBet, false, stance);
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
            var stance = response switch
            {
                BetResponse.Shield => RoundStance.Shield,
                _ => RoundStance.Stand
            };

            bettingSystem.PlaceBets(nextBet, doubled, stance);
            miniGame.PlayRound();
        }

        public void Leave()
        {
            if (GameManager.Instance?.OxygenTank != null)
                GameManager.Instance.OxygenTank.IsActiveDrain = false;

            PersistTableState();
            miniGame.End();
            ElevatorSceneLoader.ReturnToHub();
        }

        void BeginSession()
        {
            ApplyTableProfile();
            // Each seating is a closed duel: the dealer sits down with a full stack every
            // time. Persisting it let the player bank progress by standing up while ahead
            // and grind any table down with no risk.
            // The table sets the floor of the stakes; the dealer's remembered raise can only push it up.
            int minimumBet = HasTable
                ? Mathf.Max(ElevatorSceneLoader.CurrentTableMinimumBet, TableState.GetMinimumBet(TableId))
                : 0;
            bettingSystem.Initialize(minimumBet);

            if (bettingSystem.IsPlayerBroke)
            {
                OnSessionStarted?.Invoke();
                bettingSystem.ForceGameOver(false);
                return;
            }

            if (GameManager.Instance?.OxygenTank != null)
                GameManager.Instance.OxygenTank.IsActiveDrain = true;

            miniGame.Begin();
            if (HasTable && TableState.HasStandingProposal(TableId)) bettingSystem.RestoreProposal();
            OnSessionStarted?.Invoke();
        }

        void OnRoundResolved(RoundResult result)
        {
            bettingSystem.ResolveResult(result.Outcome);
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

        // The table decides who deals: name, bankroll, brain and (for RPS) cheating.
        void ApplyTableProfile()
        {
            var profile = ElevatorSceneLoader.CurrentTableProfile;
            if (profile == null) return;

            var dealer = profile.DealerFor(TableId);
            bettingSystem.Opponent?.Configure(dealer);
            if (miniGame is RPS.RPSGame rps && dealer != null) rps.SetCheatChance(dealer.cheatChance);
        }

        // The table remembers the opponent's stack and its last proposal, so folding
        // and re-entering cannot reset the stakes (see docs: "Design note: folding").
        void PersistTableState()
        {
            if (!HasTable) return;
            int minimum = Mathf.Min(bettingSystem.NpcProposedBet, bettingSystem.OpponentCoins);
            TableState.Save(TableId, minimum, bettingSystem.HasStandingProposal);
        }
    }
}
