using System;
using UnityEngine;
using UnityEngine.InputSystem;
using EcosDelAzar.Core;
using EcosDelAzar.Elevator;
using EcosDelAzar.UI;

namespace EcosDelAzar.MiniGames
{
    public class MinigameEntryTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] string minigameSceneName;
        [SerializeField] InputActionReference interactAction;
        [SerializeField] int minimumBetRequired = 10;

        bool playerInRange;

        public bool PlayerInRange => playerInRange;
        public bool CanAfford => GameManager.Instance?.Wallet != null
            && GameManager.Instance.Wallet.Coins >= minimumBetRequired;

        public event Action<bool> OnPlayerRangeChanged;
        public event Action OnInteractionBlocked;

        void OnEnable()
        {
            if (interactAction?.action != null)
            {
                interactAction.action.performed += OnInteract;
                interactAction.action.Enable();
            }
        }

        void OnDisable()
        {
            if (interactAction?.action != null)
                interactAction.action.performed -= OnInteract;
        }

        void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!playerInRange) return;

            if (!CanAfford)
            {
                OnInteractionBlocked?.Invoke();
                return;
            }

            ElevatorSceneLoader.LoadMinigame(minigameSceneName);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            OnPlayerRangeChanged?.Invoke(true);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            OnPlayerRangeChanged?.Invoke(false);
        }
    }
}
