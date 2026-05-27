using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.Elevator;
using EcosDelAzar.UI;

namespace EcosDelAzar.MiniGames
{
    public class MinigameEntryTrigger : InteractableBase
    {
        [SerializeField] string minigameSceneName;
        [SerializeField] int minimumBetRequired = 10;

        public bool CanAfford => GameManager.Instance?.Wallet != null
            && GameManager.Instance.Wallet.Coins >= minimumBetRequired;

        protected override void OnInteract()
        {
            if (!CanAfford)
            {
                RaiseInteractionBlocked();
                return;
            }

            RaiseInteractionStarted();
            ElevatorSceneLoader.LoadMinigame(minigameSceneName);
        }
    }
}
