using UnityEngine;
using UnityEngine.InputSystem;
using EcosDelAzar.AI;
using EcosDelAzar.MiniGames;

namespace EcosDelAzar.NPC
{
    public class DuelNPC : MonoBehaviour
    {
        [SerializeField] string npcId = "npc_001";
        [SerializeField] InputActionReference interactAction;

        bool playerInRange;
        DuelManager duelManager;
        BettingManager bettingManager;

        void Awake()
        {
            duelManager = FindFirstObjectByType<DuelManager>();
            bettingManager = FindFirstObjectByType<BettingManager>();
        }

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
            if (!playerInRange) return;
            if (duelManager == null || duelManager.DuelInProgress) return;
            if (bettingManager != null && bettingManager.PlayerChips < bettingManager.minimumBet) return;

            duelManager.StartDuel(npcId);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) playerInRange = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) playerInRange = false;
        }
    }
}
