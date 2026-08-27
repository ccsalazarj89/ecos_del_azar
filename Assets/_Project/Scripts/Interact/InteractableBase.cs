using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EcosDelAzar.UI
{
    /// <summary>
    /// Reusable base for objects the player can interact with through a proximity
    /// trigger plus an input action. Subclasses only implement <see cref="OnInteract"/>;
    /// range tracking, input wiring and the IInteractable events live here.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] protected InputActionReference interactAction;

        public bool PlayerInRange { get; private set; }

        /// <summary>Subclasses return a hint that replaces the label default (e.g. "Mesa vacía").</summary>
        public virtual string HintOverride => null;

        public event Action<bool> OnPlayerRangeChanged;
        public event Action OnInteractionStarted;
        public event Action OnInteractionBlocked;

        protected virtual void OnEnable()
        {
            if (interactAction?.action == null) return;
            interactAction.action.performed += HandleInteractInput;
            interactAction.action.Enable();
        }

        protected virtual void OnDisable()
        {
            if (interactAction?.action != null)
                interactAction.action.performed -= HandleInteractInput;
        }

        void HandleInteractInput(InputAction.CallbackContext ctx)
        {
            // Dead, paused or in the menu: the world stops listening.
            var gm = Core.GameManager.Instance;
            if (gm != null && gm.State != Core.GameState.Playing) return;
            if (PlayerInRange) OnInteract();
        }

        /// <summary>Runs when the player triggers the action while in range.</summary>
        protected abstract void OnInteract();

        protected void RaiseInteractionStarted() => OnInteractionStarted?.Invoke();

        protected void RaiseInteractionBlocked() => OnInteractionBlocked?.Invoke();

        /// <summary>Re-emits the in-range state so the hint reappears after an interaction ends.</summary>
        protected void NotifyHintVisible()
        {
            if (PlayerInRange) OnPlayerRangeChanged?.Invoke(true);
        }

        void SetPlayerInRange(bool inRange)
        {
            if (PlayerInRange == inRange) return;
            PlayerInRange = inRange;
            OnPlayerRangeChanged?.Invoke(inRange);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) SetPlayerInRange(true);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            SetPlayerInRange(false);
            OnPlayerExitRange();
        }

        /// <summary>Hook for subclasses to react when the player leaves the trigger.</summary>
        protected virtual void OnPlayerExitRange() { }
    }
}
