using UnityEngine;
using UnityEngine.SceneManagement;
using EcosDelAzar.Core;
using EcosDelAzar.Elevator;
using EcosDelAzar.UI;

namespace EcosDelAzar.MiniGames
{
    /// <summary>
    /// A gambling table in a hub. Loads the minigame scene when the player can
    /// afford the table's current minimum; closed for the run once its opponent
    /// has been bankrupted (that earns a house chip).
    /// </summary>
    public class MinigameEntryTrigger : InteractableBase
    {
        const string BeatenHint = "Mesa vacía";

        [SerializeField] string minigameSceneName;
        [SerializeField] int minimumBetRequired = 10;

        [Header("Table identity")]
        [Tooltip("Unique per run. Leave empty to use <scene>/<object name>.")]
        [SerializeField] string tableId;
        [Tooltip("Floor number used for house-chip requirements (boss needs chips from floor 2+).")]
        [SerializeField] int floorNumber = 1;

        public string TableId => string.IsNullOrEmpty(tableId)
            ? $"{SceneManager.GetActiveScene().name}/{name}"
            : tableId;

        public bool IsBeaten => TableState.IsBeaten(TableId);

        /// <summary>The table's live minimum: its default, or the opponent's last proposal if higher.</summary>
        public int CurrentMinimumBet => Mathf.Max(minimumBetRequired, TableState.GetMinimumBet(TableId));

        public bool CanAfford => GameManager.Instance?.Wallet != null
            && GameManager.Instance.Wallet.Coins >= CurrentMinimumBet;

        public override string HintOverride => IsBeaten ? BeatenHint : null;

        protected override void OnInteract()
        {
            if (IsBeaten || !CanAfford)
            {
                RaiseInteractionBlocked();
                return;
            }

            RaiseInteractionStarted();
            ElevatorSceneLoader.LoadMinigame(minigameSceneName, TableId, floorNumber);
        }
    }
}
