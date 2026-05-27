using System;
using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.MiniGames.Betting;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.MiniGames
{
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

            int nextBet = response == BetResponse.Double
                ? bettingSystem.LastBet * 2
                : bettingSystem.NpcProposedBet;

            bettingSystem.PlaceBets(nextBet);
            miniGame.PlayRound();
        }

        public void Leave()
        {
            if (GameManager.Instance?.OxygenTank != null)
                GameManager.Instance.OxygenTank.IsActiveDrain = false;

            miniGame.End();
            OnSessionEnded?.Invoke();
            ElevatorSceneLoader.ReturnToHub();
        }

        void BeginSession()
        {
            RoundsPlayed = 0;
            bettingSystem.Initialize();

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
            // UI handles showing game over screen — don't auto-leave
        }
    }
}
