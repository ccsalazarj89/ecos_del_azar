using System;
using UnityEngine;
using EcosDelAzar.UI;

namespace EcosDelAzar.NPC
{
    public class DialogueNPC : InteractableBase
    {
        [SerializeField] DialogueLine[] lines;
        [SerializeField] bool oneTimeOnly = true;

        bool dialogueActive;
        bool completed;
        int currentLine;

        public bool DialogueActive => dialogueActive;
        public bool Completed => completed;

        public event Action OnDialogueStarted;
        public event Action<DialogueLine> OnLineShown;
        public event Action OnDialogueEnded;

        protected override void OnInteract()
        {
            if (!dialogueActive)
            {
                if (oneTimeOnly && completed)
                {
                    RaiseInteractionBlocked();
                    return;
                }
                StartDialogue();
                return;
            }

            AdvanceLine();
        }

        protected override void OnPlayerExitRange()
        {
            if (dialogueActive) EndDialogue();
        }

        void StartDialogue()
        {
            dialogueActive = true;
            currentLine = 0;
            RaiseInteractionStarted();
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

            // Re-show the hint if the NPC can still be interacted with.
            if (!(oneTimeOnly && completed)) NotifyHintVisible();
        }
    }
}
