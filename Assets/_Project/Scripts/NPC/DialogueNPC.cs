using System;
using UnityEngine;
using EcosDelAzar.UI;

namespace EcosDelAzar.NPC
{
    public class DialogueNPC : InteractableBase
    {
        [SerializeField] DialogueLine[] lines;
        [SerializeField] bool oneTimeOnly = true;
        [Tooltip("When set, the completed state survives scene reloads (PlayerPrefs). Keys are prefixed with \"run.\" so a run reset can clear them all.")]
        [SerializeField] string persistenceId;

        bool dialogueActive;
        bool completed;
        int currentLine;

        public bool DialogueActive => dialogueActive;
        public bool Completed => completed;
        public string PersistenceId => persistenceId;

        const string PrefsPrefix = "run.dialogue.";

        void Awake()
        {
            if (oneTimeOnly && !string.IsNullOrEmpty(persistenceId))
                completed = PlayerPrefs.GetInt(PrefsPrefix + persistenceId, 0) == 1;
        }

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
            if (oneTimeOnly && !string.IsNullOrEmpty(persistenceId))
            {
                PlayerPrefs.SetInt(PrefsPrefix + persistenceId, 1);
                PlayerPrefs.Save();
            }
            OnDialogueEnded?.Invoke();

            // Re-show the hint if the NPC can still be interacted with.
            if (!(oneTimeOnly && completed)) NotifyHintVisible();
        }
    }
}
