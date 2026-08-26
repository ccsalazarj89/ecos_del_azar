using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.Core;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHUD : MonoBehaviour
    {
        const float LowOxygenThreshold = 0.35f;
        const float CriticalOxygenThreshold = 0.15f;
        const int ChipSlots = 3;
        const float AnnouncementSeconds = 6f;

        [Header("House announcements (index = chips earned - 1)")]
        [SerializeField] string[] chipAnnouncements =
        {
            "Atención: un cliente acaba de vaciar una mesa. Que nadie le sirva más aire.",
            "El director quiere ver de cerca al del tanque a la espalda.",
            "Planta preferencial abierta. Suba cuando quiera... si le queda aire."
        };

        VisualElement hudContainer;
        Label coinsLabel;
        Label oxygenPercent;
        VisualElement oxygenBarFill;
        VisualElement oxygenModule;
        VisualElement[] chipSlots = new VisualElement[ChipSlots];
        VisualElement announcement;
        Label announcementText;

        GameManager gameManager;
        Wallet wallet;
        OxygenTank oxygenTank;
        Coroutine announcementRoutine;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            var root = doc.rootVisualElement;
            hudContainer = root.Q("HUDContainer");
            coinsLabel = root.Q<Label>("coins-value");
            oxygenPercent = root.Q<Label>("oxygen-percent");
            oxygenBarFill = root.Q("oxygen-fill");
            oxygenModule = root.Q("OxygenModule");
            announcement = root.Q("Announcement");
            announcementText = root.Q<Label>("announcement-text");
            for (int i = 0; i < ChipSlots; i++)
                chipSlots[i] = root.Q($"chip-{i}");

            HouseChips.OnChipsChanged += OnChipsChanged;
            announcement?.RegisterCallback<ClickEvent>(OnAnnouncementClicked);
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager == null) return;

            wallet = gameManager.Wallet;
            oxygenTank = gameManager.OxygenTank;

            if (wallet != null)
            {
                wallet.OnCoinsChanged += UpdateCoins;
                UpdateCoins(wallet.Coins);
            }

            if (oxygenTank != null)
            {
                oxygenTank.OnOxygenChanged += UpdateOxygen;
                UpdateOxygen(oxygenTank.Current);
            }

            gameManager.OnStateChanged += UpdateVisibility;
            UpdateVisibility(gameManager.State);
            RefreshChips(HouseChips.Count);
        }

        void OnDisable()
        {
            if (wallet != null)
                wallet.OnCoinsChanged -= UpdateCoins;

            if (oxygenTank != null)
                oxygenTank.OnOxygenChanged -= UpdateOxygen;

            if (gameManager != null)
                gameManager.OnStateChanged -= UpdateVisibility;

            HouseChips.OnChipsChanged -= OnChipsChanged;
            announcement?.UnregisterCallback<ClickEvent>(OnAnnouncementClicked);
        }

        void UpdateVisibility(GameState state)
        {
            if (hudContainer == null) return;

            // The in-game HUD makes no sense on the main menu.
            hudContainer.style.display = state == GameState.MainMenu
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            // A fresh run may have been started from the menu.
            if (state == GameState.Playing) RefreshChips(HouseChips.Count);
        }

        void UpdateCoins(int amount)
        {
            if (coinsLabel != null)
                coinsLabel.text = amount.ToString();
        }

        void UpdateOxygen(float currentOxygen)
        {
            if (oxygenTank == null) return;

            float ratio = oxygenTank.Ratio;

            if (oxygenBarFill != null)
            {
                oxygenBarFill.style.width = new Length(ratio * 100f, LengthUnit.Percent);

                oxygenBarFill.EnableInClassList("oxygen-low", ratio <= LowOxygenThreshold && ratio > CriticalOxygenThreshold);
                oxygenBarFill.EnableInClassList("oxygen-critical", ratio <= CriticalOxygenThreshold);
            }

            if (oxygenPercent != null)
                oxygenPercent.text = $"{Mathf.RoundToInt(ratio * 100f)}%";

            if (oxygenModule != null)
                oxygenModule.EnableInClassList("oxygen-critical-state", ratio <= CriticalOxygenThreshold);
        }

        void OnChipsChanged(int count)
        {
            RefreshChips(count);

            int index = Mathf.Clamp(count - 1, 0, chipAnnouncements.Length - 1);
            if (count >= 1 && chipAnnouncements.Length > 0)
                Announce(chipAnnouncements[index]);
        }

        void RefreshChips(int count)
        {
            for (int i = 0; i < ChipSlots; i++)
                chipSlots[i]?.EnableInClassList("chip-slot--earned", i < count);
        }

        void Announce(string message)
        {
            if (announcement == null || announcementText == null) return;
            if (announcementRoutine != null) StopCoroutine(announcementRoutine);
            announcementRoutine = StartCoroutine(AnnounceRoutine(message));
        }

        IEnumerator AnnounceRoutine(string message)
        {
            announcementText.text = message;
            announcement.RemoveFromClassList("announcement--hidden");
            yield return new WaitForSecondsRealtime(AnnouncementSeconds);
            HideAnnouncement();
        }

        void OnAnnouncementClicked(ClickEvent _) => HideAnnouncement();

        void HideAnnouncement()
        {
            if (announcementRoutine != null) StopCoroutine(announcementRoutine);
            announcementRoutine = null;
            announcement?.AddToClassList("announcement--hidden");
        }
    }
}
