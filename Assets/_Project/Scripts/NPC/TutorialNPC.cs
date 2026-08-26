using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.NPC
{
    /// <summary>
    /// Pays a one-time coin reward when its dialogue ends. The "already paid"
    /// flag is persisted per run (PlayerPrefs, "run." prefix) so reloading the
    /// floor cannot be used to farm the concierge.
    /// </summary>
    [RequireComponent(typeof(DialogueNPC))]
    public class TutorialNPC : MonoBehaviour
    {
        const string PrefsPrefix = "run.reward.";

        [SerializeField] int coinsReward = 50;

        DialogueNPC dialogue;

        string PrefsKey => PrefsPrefix + (string.IsNullOrEmpty(dialogue.PersistenceId) ? name : dialogue.PersistenceId);
        bool Rewarded
        {
            get => PlayerPrefs.GetInt(PrefsKey, 0) == 1;
            set { PlayerPrefs.SetInt(PrefsKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        void Awake()
        {
            dialogue = GetComponent<DialogueNPC>();
        }

        void OnEnable()
        {
            dialogue.OnDialogueEnded += GiveReward;
        }

        void OnDisable()
        {
            dialogue.OnDialogueEnded -= GiveReward;
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
