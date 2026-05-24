using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.NPC
{
    [RequireComponent(typeof(DialogueNPC))]
    public class TutorialNPC : MonoBehaviour
    {
        [SerializeField] int coinsReward = 50;

        DialogueNPC dialogue;
        bool rewarded;

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
            if (rewarded) return;
            rewarded = true;

            var wallet = GameManager.Instance?.Wallet;
            if (wallet != null)
                wallet.Add(coinsReward);
        }
    }
}
