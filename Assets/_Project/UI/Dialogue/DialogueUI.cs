using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.NPC;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] DialogueNPC targetNPC;
        [SerializeField] string speakerName = "???";

        [Header("Prompt Texts")]
        [SerializeField] string continuePrompt = "[E] Continuar";
        [SerializeField] string rewardPrompt = "[E] Recibir";

        VisualElement root;
        Label speakerLabel;
        Label textLabel;
        VisualElement promptElement;
        Label promptLabel;

        const string HiddenClass = "dialogue-hidden";

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            root = doc.rootVisualElement.Q("dialogue-root");
            speakerLabel = doc.rootVisualElement.Q<Label>("speaker-name");
            textLabel = doc.rootVisualElement.Q<Label>("dialogue-text");
            promptElement = doc.rootVisualElement.Q("dialogue-prompt");
            promptLabel = doc.rootVisualElement.Q<Label>("dialogue-prompt-label");

            if (targetNPC != null)
            {
                targetNPC.OnDialogueStarted += OnDialogueStarted;
                targetNPC.OnLineShown += OnLine;
                targetNPC.OnDialogueEnded += Hide;
            }

            Hide();
        }

        void OnDisable()
        {
            if (targetNPC != null)
            {
                targetNPC.OnDialogueStarted -= OnDialogueStarted;
                targetNPC.OnLineShown -= OnLine;
                targetNPC.OnDialogueEnded -= Hide;
            }
        }

        void OnDialogueStarted()
        {
            if (root != null) root.RemoveFromClassList(HiddenClass);
            if (speakerLabel != null) speakerLabel.text = speakerName;
        }

        void OnLine(DialogueLine line)
        {
            if (textLabel != null)
                textLabel.text = line.text;

            if (promptElement != null)
                promptElement.RemoveFromClassList(HiddenClass);

            if (promptLabel != null)
                promptLabel.text = line.isReward ? rewardPrompt : continuePrompt;
        }

        void Hide()
        {
            if (root != null) root.AddToClassList(HiddenClass);
        }
    }
}
