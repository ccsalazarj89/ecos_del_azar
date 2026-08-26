using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.NPC
{
    /// <summary>
    /// Pays a coin reward the first time its dialogue is seen through to the end.
    /// The paid flag lives in RunPrefs, so reloading the floor cannot farm it.
    /// </summary>
    [RequireComponent(typeof(DialogueNPC))]
    public class TutorialNPC : MonoBehaviour
    {
        const string PrefsPrefix = "reward.";

        [SerializeField] int coinsReward = 50;

        DialogueNPC dialogue;

        string PrefsKey => PrefsPrefix + (string.IsNullOrEmpty(dialogue.PersistenceId) ? name : dialogue.PersistenceId);
        bool Rewarded
        {
            get => RunPrefs.GetInt(PrefsKey, 0) == 1;
            set { RunPrefs.SetInt(PrefsKey, value ? 1 : 0); RunPrefs.Save(); }
        }

        void Awake()
        {
            dialogue = GetComponent<DialogueNPC>();
        }

        void OnEnable()
        {
            dialogue.OnDialogueCompleted += GiveReward;
        }

        void OnDisable()
        {
            dialogue.OnDialogueCompleted -= GiveReward;
        }

        void GiveReward()
        {
            if (Rewarded) return;

            var wallet = GameManager.Instance?.Wallet;
            if (wallet == null) return;

            Rewarded = true;
            wallet.Add(coinsReward);
        }
    }
}
