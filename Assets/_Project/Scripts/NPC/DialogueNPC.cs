using System;
using UnityEngine;
using UnityEngine.InputSystem;
using EcosDelAzar.UI;

namespace EcosDelAzar.NPC
{
    public class DialogueNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] InputActionReference interactAction;
        [SerializeField] DialogueLine[] lines;
        [SerializeField] bool oneTimeOnly = true;

        bool playerInRange;
        bool dialogueActive;
        bool completed;
        int currentLine;

        public bool PlayerInRange => playerInRange;
        public bool DialogueActive => dialogueActive;
        public bool Completed => completed;

        public event Action<bool> OnPlayerRangeChanged;
        public event Action OnInteractionBlocked;
        public event Action OnDialogueStarted;
        public event Action<DialogueLine> OnLineShown;
        public event Action OnDialogueEnded;

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
        }

        void OnInteract(InputAction.CallbackContext ctx)
        {
            if (!playerInRange) return;

            if (!dialogueActive)
            {
                if (oneTimeOnly && completed)
                {
                    OnInteractionBlocked?.Invoke();
                    return;
                }
                StartDialogue();
                return;
            }

            AdvanceLine();
        }

        void StartDialogue()
        {
            dialogueActive = true;
            currentLine = 0;
            OnDialogueStarted?.Invoke();
            ShowCurrentLine();
        }

        void ShowCurrentLine()
        {
            if (currentLine >= lines.Length)
            {
                EndDialogue();
                return;
            }

            OnLineShown?.Invoke(lines[currentLine]);
        }

        void AdvanceLine()
        {
            currentLine++;
            ShowCurrentLine();
        }

        void EndDialogue()
        {
            dialogueActive = false;
            completed = true;
            OnDialogueEnded?.Invoke();
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

            if (dialogueActive)
                EndDialogue();
        }
    }
}
