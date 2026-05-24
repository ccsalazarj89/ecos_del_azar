using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.AI;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class BettingUI : MonoBehaviour
    {
        [SerializeField] BettingManager bettingManager;

        VisualElement root;
        Label playerChipsLabel;
        Label npcChipsLabel;
        Label minBetLabel;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>().rootVisualElement;
            root = doc.Q("betting-panel");
            playerChipsLabel = doc.Q<Label>("player-chips");
            npcChipsLabel = doc.Q<Label>("npc-chips");
            minBetLabel = doc.Q<Label>("min-bet");

            doc.Q<Button>("btn-equal").clicked += () => Act(BetAction.Equal);
            doc.Q<Button>("btn-double").clicked += () => Act(BetAction.Double);
            doc.Q<Button>("btn-allin").clicked += () => Act(BetAction.AllIn);
            doc.Q<Button>("btn-fold").clicked += () => Act(BetAction.FoldRound);
            doc.Q<Button>("btn-abandon").clicked += () => Act(BetAction.AbandonGame);

            bettingManager.OnBetConfirmed += (_, __) => Hide();
            bettingManager.OnRoundFolded += Hide;
            bettingManager.OnGameAbandoned += Hide;
            bettingManager.OnGameOver += Hide;

            Hide();
        }

        public void Show()
        {
            Refresh();
            root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            root.style.display = DisplayStyle.None;
        }

        void Act(BetAction action) => bettingManager.ProcessPlayerAction(action);

        void Refresh()
        {
            playerChipsLabel.text = bettingManager.PlayerChips.ToString();
            npcChipsLabel.text = bettingManager.NpcChips.ToString();
            minBetLabel.text = bettingManager.minimumBet.ToString();
        }
    }
}
