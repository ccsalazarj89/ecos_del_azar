using EcosDelAzar.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcosDelAzar.Elevator
{
    public class Elevator : MonoBehaviour
    {
        [SerializeField] ElevatorUI elevatorUI;
        [SerializeField] ElevatorFloorData[] floors;
        [SerializeField] InputActionReference interactAction;

        bool playerInside;

        void OnEnable()
        {
            if (interactAction?.action == null) return;
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }

        void OnDisable()
        {
            if (interactAction?.action == null) return;
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }

        void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!playerInside || elevatorUI == null) return;
            elevatorUI.Open(floors);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) playerInside = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInside = false;
            elevatorUI?.Close();
        }
    }
}
