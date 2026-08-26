using EcosDelAzar.Core;
using EcosDelAzar.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcosDelAzar.Elevator
{
    public class Elevator : InteractableBase
    {
        [SerializeField] ElevatorUI elevatorUI;
        [SerializeField] ElevatorFloorData[] floors;
        [SerializeField] InputActionReference exitAction;

        public override string HintOverride => TutorialProgress.ElevatorLocked ? TutorialProgress.ElevatorLockedHint : null;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (exitAction?.action == null) return;
            exitAction.action.performed += OnExit;
            exitAction.action.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (exitAction?.action != null)
                exitAction.action.performed -= OnExit;
        }

        protected override void OnInteract()
        {
            if (elevatorUI == null) return;
            if (TutorialProgress.ElevatorLocked)
            {
                RaiseInteractionBlocked();
                return;
            }
            RaiseInteractionStarted();
            elevatorUI.Open(floors);
        }

        protected override void OnPlayerExitRange()
        {
            elevatorUI?.Close();
        }

        void OnExit(InputAction.CallbackContext ctx)
        {
            if (elevatorUI == null) return;
            elevatorUI.Close();

            // Bring the hint back if the player is still standing inside.
            NotifyHintVisible();
        }
    }
}
