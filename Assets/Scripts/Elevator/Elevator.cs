using UnityEngine;
using UnityEngine.InputSystem;

public class Elevator : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ElevatorUI elevatorUI;

    [Header("Floors")]
    [SerializeField] private ElevatorFloorData[] floors;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    private bool playerInside;

    private void OnEnable()
    {
        if (interactAction == null || interactAction.action == null)
        {
            Debug.LogWarning("Interact action is not assigned in Elevator.");
            return;
        }

        interactAction.action.performed += OnInteractPerformed;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction == null || interactAction.action == null)
        {
            return;
        }

        interactAction.action.performed -= OnInteractPerformed;
        interactAction.action.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!playerInside)
        {
            return;
        }

        if (elevatorUI == null)
        {
            Debug.LogWarning("ElevatorUI is not assigned.");
            return;
        }

        elevatorUI.Open(floors);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;

        if (elevatorUI != null)
        {
            elevatorUI.Close();
        }
    }
}