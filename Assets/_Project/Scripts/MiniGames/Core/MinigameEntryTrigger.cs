using UnityEngine;
using UnityEngine.SceneManagement;
using EcosDelAzar.Core;
using EcosDelAzar.Elevator;
using EcosDelAzar.Player;
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
        [Tooltip("Dealer, bankroll and stakes for this table. Overrides the scene defaults.")]
        [SerializeField] TableProfile profile;

        [Header("Table identity")]
        [Tooltip("Unique per run. Leave empty to use <scene>/<object name>.")]
        [SerializeField] string tableId;
        [Tooltip("Floor number used for house-chip requirements (boss needs chips from floor 2+).")]
        [SerializeField] int floorNumber = 1;
        [Tooltip("Bankrupting this table's dealer earns a house chip. Off for the boss table.")]
        [SerializeField] bool awardsHouseChip = true;
        [Tooltip("Where the player sits. The player walks here and sits before the game loads, and stands up from here on return.")]
        [SerializeField] Transform seatAnchor;
        [Tooltip("Optional. Where the player ends up after standing (e.g. beside the chair). Empty = stays at the seat.")]
        [SerializeField] Transform standAnchor;

        public string TableId => string.IsNullOrEmpty(tableId)
            ? $"{SceneManager.GetActiveScene().name}/{name}"
            : tableId;

        public bool IsBeaten => TableState.IsBeaten(TableId);
        public Transform SeatAnchor => seatAnchor;
        public Transform StandAnchor => standAnchor;

        /// <summary>The table's live minimum: its default, or the opponent's last proposal if higher.</summary>
        int BaseMinimumBet => profile != null ? profile.MinimumBet : minimumBetRequired;
        public int CurrentMinimumBet => Mathf.Max(BaseMinimumBet, TableState.GetMinimumBet(TableId));

        public bool CanAfford => GameManager.Instance?.Wallet != null
            && GameManager.Instance.Wallet.Coins >= CurrentMinimumBet;

        public override string HintOverride => IsBeaten ? BeatenHint : null;

        public override string BlockedReason
        {
            get
            {
                if (IsBeaten) return "Mesa vacía — busca otra";
                int coins = GameManager.Instance?.Wallet?.Coins ?? 0;
                return $"Te faltan {CurrentMinimumBet - coins} monedas (mínimo {CurrentMinimumBet}). Vende oxígeno en la máquina de O2";
            }
        }

        protected override void OnInteract()
        {
            if (IsBeaten || !CanAfford)
            {
                RaiseInteractionBlocked();
                return;
            }

            var seating = FindFirstObjectByType<PlayerSeating>();
            if (seating != null && seating.IsBusy) return;

            RaiseInteractionStarted();
            if (seating != null && seatAnchor != null)
                seating.SitAt(seatAnchor, LoadGame);
            else
                LoadGame();
        }

        void LoadGame()
        {
            TutorialProgress.Advance(TutorialProgress.Stage.Done);
            ElevatorSceneLoader.LoadMinigame(minigameSceneName, TableId, floorNumber, awardsHouseChip, BaseMinimumBet, profile);
        }

        void OnDrawGizmosSelected()
        {
            if (seatAnchor != null)
            {
                Gizmos.color = new Color(0.2f, 0.7f, 1f);
                Gizmos.DrawWireSphere(seatAnchor.position, 0.25f);
                Gizmos.DrawLine(seatAnchor.position, seatAnchor.position + seatAnchor.forward * 0.6f);
            }

            if (standAnchor != null)
            {
                Gizmos.color = new Color(0.3f, 1f, 0.4f);
                Gizmos.DrawWireSphere(standAnchor.position, 0.25f);
                if (seatAnchor != null) Gizmos.DrawLine(seatAnchor.position, standAnchor.position);
            }
        }
    }
}
