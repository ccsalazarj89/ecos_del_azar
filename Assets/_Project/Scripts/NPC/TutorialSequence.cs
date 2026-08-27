using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.Vending;

namespace EcosDelAzar.NPC
{
    /// <summary>
    /// Drives the lobby tutorial: move → talk to the concierge → trade at the O2
    /// machine → use the elevator. The last step (sit at a table) completes on
    /// floor 1, from MinigameEntryTrigger. Finds its actors at runtime (one of each
    /// per lobby), so the scene only needs this component on any object. Runs once per run.
    /// </summary>
    public class TutorialSequence : MonoBehaviour
    {
        [Tooltip("Distance the player must walk before the first objective is considered done.")]
        [SerializeField] float moveDistance = 2f;

        Transform player;
        Vector3 startPosition;
        DialogueNPC concierge;
        OxygenVendingMachine machine;
        Elevator.Elevator elevator;

        void Start()
        {
            if (TutorialProgress.IsDone)
            {
                enabled = false;
                return;
            }

            var playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
            startPosition = player != null ? player.position : Vector3.zero;

            var tutorialNpc = FindFirstObjectByType<TutorialNPC>();
            concierge = tutorialNpc != null ? tutorialNpc.GetComponent<DialogueNPC>() : null;
            machine = FindFirstObjectByType<OxygenVendingMachine>();
            elevator = FindFirstObjectByType<Elevator.Elevator>();

            if (concierge != null) concierge.OnDialogueCompleted += OnConciergeDone;
            if (machine != null) machine.OnTradeCompleted += OnTraded;
            if (elevator != null) elevator.OnInteractionStarted += OnElevatorUsed;

            TutorialProgress.Rebroadcast();
        }

        void OnDestroy()
        {
            if (concierge != null) concierge.OnDialogueCompleted -= OnConciergeDone;
            if (machine != null) machine.OnTradeCompleted -= OnTraded;
            if (elevator != null) elevator.OnInteractionStarted -= OnElevatorUsed;
        }

        void Update()
        {
            if (TutorialProgress.Current != TutorialProgress.Stage.Move || player == null) return;

            if ((player.position - startPosition).sqrMagnitude >= moveDistance * moveDistance)
                TutorialProgress.Advance(TutorialProgress.Stage.TalkToConcierge);
        }

        void OnConciergeDone() => TutorialProgress.Advance(TutorialProgress.Stage.TradeOxygen);

        void OnTraded(TradeQuote _) => TutorialProgress.Advance(TutorialProgress.Stage.UseElevator);

        void OnElevatorUsed()
        {
            TutorialProgress.Advance(TutorialProgress.Stage.PlayTable);
            enabled = false;
        }
    }
}
