using System;
using UnityEngine;
using UnityEngine.InputSystem;
using EcosDelAzar.Core;
using EcosDelAzar.UI;

namespace EcosDelAzar.NPC
{
    /// <summary>
    /// Interactable that plays a sequence of lines. E advances; Esc or walking
    /// away closes. With oneTimeOnly, the first full playthrough is remembered
    /// for the run; later visits play repeatLines (or block when there are none).
    /// </summary>
    public class DialogueNPC : InteractableBase
    {
        const string PrefsPrefix = "dialogue.";

        [SerializeField] DialogueLine[] lines;
        [SerializeField] bool oneTimeOnly = true;
        [Tooltip("When set, the completed state survives scene reloads for the current run (RunPrefs).")]
        [SerializeField] string persistenceId;
        [Tooltip("Played on later visits when oneTimeOnly is set. Empty = block with the label's blocked text.")]
        [SerializeField] DialogueLine[] repeatLines;
        [SerializeField] InputActionReference exitAction;

        bool dialogueActive;
        bool completed;
        int currentLine;

        public bool DialogueActive => dialogueActive;
        public bool Completed => completed;
        public string PersistenceId => persistenceId;

        public event Action OnDialogueStarted;
        public event Action<DialogueLine> OnLineShown;
        /// <summary>Fires every time the panel closes, including early exits.</summary>
        public event Action OnDialogueEnded;
        /// <summary>Fires once per run, when the main lines are seen through to the end.</summary>
        public event Action OnDialogueCompleted;

        bool HasRepeatLines => repeatLines != null && repeatLines.Length > 0;
        bool Persisted => oneTimeOnly && !string.IsNullOrEmpty(persistenceId);
        DialogueLine[] ActiveLines => oneTimeOnly && completed && HasRepeatLines ? repeatLines : lines;

        void Awake()
        {
            if (Persisted)
                completed = RunPrefs.GetInt(PrefsPrefix + persistenceId, 0) == 1;
        }

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
            if (!dialogueActive)
            {
                if (oneTimeOnly && completed && !HasRepeatLines)
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

        void OnExit(InputAction.CallbackContext _)
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
            var active = ActiveLines;
            if (currentLine >= active.Length)
            {
                EndDialogue(reachedEnd: true);
                return;
            }

            OnLineShown?.Invoke(active[currentLine]);
        }

        void AdvanceLine()
        {
            currentLine++;
            ShowCurrentLine();
        }

        void EndDialogue(bool reachedEnd = false)
        {
            dialogueActive = false;

            if (reachedEnd && !completed)
            {
                completed = true;
                if (Persisted)
                {
                    RunPrefs.SetInt(PrefsPrefix + persistenceId, 1);
                    RunPrefs.Save();
                }
                OnDialogueCompleted?.Invoke();
            }

            OnDialogueEnded?.Invoke();

            // Re-show the hint if the NPC can still be talked to.
            if (!oneTimeOnly || !completed || HasRepeatLines) NotifyHintVisible();
        }
    }
}
