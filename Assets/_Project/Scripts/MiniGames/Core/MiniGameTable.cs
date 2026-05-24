using System;
using UnityEngine;
using UnityEngine.InputSystem;
using EcosDelAzar.MiniGames.Betting;

namespace EcosDelAzar.MiniGames
{
    // Place on the table GameObject in the scene.
    // Wires together: MiniGame + BettingManager + player interaction.
    public class MiniGameTable : MonoBehaviour
    {
        [SerializeField] MiniGameBase miniGame;
        [SerializeField] MiniGameConfig config;
        [SerializeField] BettingManager bettingManager;
        [SerializeField] InputActionReference interactAction;

        public MiniGameBase Game => miniGame;
        public MiniGameConfig Config => config;
        public bool PlayerSeated { get; private set; }

        public event Action<MiniGameTable> OnPlayerSat;
        public event Action<MiniGameTable> OnPlayerLeft;

        bool playerInRange;
        int roundsPlayed;

        void OnEnable()
        {
            if (interactAction?.action == null) return;
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();

            if (miniGame != null)
                miniGame.OnRoundResolved += OnRoundResolved;

            if (bettingManager != null)
            {
                bettingManager.OnBetConfirmed += OnBetConfirmed;
                bettingManager.OnRoundFolded += OnFoldOrAbandon;
                bettingManager.OnGameAbandoned += OnFoldOrAbandon;
                bettingManager.OnGameOver += OnGameOver;
            }
        }

        void OnDisable()
        {
            if (interactAction?.action != null)
            {
                interactAction.action.performed -= OnInteract;
                interactAction.action.Disable();
            }

            if (miniGame != null)
                miniGame.OnRoundResolved -= OnRoundResolved;

            if (bettingManager != null)
            {
                bettingManager.OnBetConfirmed -= OnBetConfirmed;
                bettingManager.OnRoundFolded -= OnFoldOrAbandon;
                bettingManager.OnGameAbandoned -= OnFoldOrAbandon;
                bettingManager.OnGameOver -= OnGameOver;
            }
        }

        void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!playerInRange || PlayerSeated) return;
            if (bettingManager.PlayerChips < config.minimumBet) return;

            Sit();
        }

        void Sit()
        {
            PlayerSeated = true;
            roundsPlayed = 0;
            miniGame.Begin();
            OnPlayerSat?.Invoke(this);
        }

        public void Leave()
        {
            if (!PlayerSeated) return;
            PlayerSeated = false;
            miniGame.End();
            OnPlayerLeft?.Invoke(this);
        }

        void OnBetConfirmed(int playerBet, int npcBet)
        {
            if (!PlayerSeated) return;
            miniGame.PlayRound();
        }

        void OnRoundResolved(RoundResult result)
        {
            bettingManager.ResolveResult(result.Outcome);
            roundsPlayed++;

            if (roundsPlayed >= config.maxRoundsPerMatch)
                Leave();
        }

        void OnFoldOrAbandon() => Leave();
        void OnGameOver() => Leave();

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) playerInRange = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            if (PlayerSeated) Leave();
        }
    }
}
