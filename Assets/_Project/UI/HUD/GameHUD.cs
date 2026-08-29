using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.Core;
using EcosDelAzar.Core.Echoes;

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
        Label oxygenWarning;
        Label oxygenEvent;
        Coroutine oxygenEventRoutine;
        Label notice;
        Coroutine noticeRoutine;
        VisualElement[] chipSlots = new VisualElement[ChipSlots];
        VisualElement announcement;
        Label announcementText;
        VisualElement echoesModule;
        VisualElement echoBadges;
        RunModifiers modifiers;
        VisualElement objective;
        Label objectiveStep;
        Label objectiveIcon;
        Label objectiveText;

        GameManager gameManager;
        Wallet wallet;
        OxygenTank oxygenTank;
        Coroutine announcementRoutine;
        int knownChips;

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
            oxygenWarning = root.Q<Label>("oxygen-warning");
            oxygenEvent = root.Q<Label>("oxygen-event");
            notice = root.Q<Label>("hud-notice");
            announcement = root.Q("Announcement");
            announcementText = root.Q<Label>("announcement-text");
            echoesModule = root.Q("EchoesModule");
            echoBadges = root.Q("echo-badges");
            objective = root.Q("Objective");
            objectiveStep = root.Q<Label>("objective-step");
            objectiveIcon = root.Q<Label>("objective-icon");
            objectiveText = root.Q<Label>("objective-text");
            for (int i = 0; i < ChipSlots; i++)
                chipSlots[i] = root.Q($"chip-{i}");

            HouseChips.OnChipsChanged += OnChipsChanged;
            announcement?.RegisterCallback<ClickEvent>(OnAnnouncementClicked);
            TutorialProgress.OnObjectiveChanged += ShowObjective;
            ShowObjective(TutorialProgress.CurrentObjectiveOrNull);
            ModalTracker.OnAnyOpenChanged += OnModalChanged;
            UINotice.OnMessage += ShowNotice;
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
            if (oxygenTank != null) oxygenTank.OnOxygenEvent += ShowOxygenEvent;

            gameManager.OnStateChanged += UpdateVisibility;
            UpdateVisibility(gameManager.State);
            RefreshChips(HouseChips.Count);

            modifiers = gameManager.Modifiers;
            if (modifiers != null)
            {
                modifiers.OnChanged += RefreshEchoes;
                modifiers.OnReviveUsed += OnReviveUsed;
                RefreshEchoes();
            }
        }

        void OnDisable()
        {
            if (wallet != null)
                wallet.OnCoinsChanged -= UpdateCoins;

            if (oxygenTank != null)
                oxygenTank.OnOxygenChanged -= UpdateOxygen;

            if (oxygenTank != null)
                oxygenTank.OnOxygenEvent -= ShowOxygenEvent;

            if (gameManager != null)
                gameManager.OnStateChanged -= UpdateVisibility;

            HouseChips.OnChipsChanged -= OnChipsChanged;
            announcement?.UnregisterCallback<ClickEvent>(OnAnnouncementClicked);
            TutorialProgress.OnObjectiveChanged -= ShowObjective;
            ModalTracker.OnAnyOpenChanged -= OnModalChanged;
            UINotice.OnMessage -= ShowNotice;

            if (modifiers != null)
            {
                modifiers.OnChanged -= RefreshEchoes;
                modifiers.OnReviveUsed -= OnReviveUsed;
            }
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

            // Overlays live outside HUDContainer (they must cover minigame UI); the menu must never inherit them.
            if (state == GameState.MainMenu)
            {
                HideAnnouncement();
                ShowObjective(null);
            }
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

            if (oxygenWarning != null)
            {
                bool critical = ratio <= CriticalOxygenThreshold;
                bool low = ratio <= LowOxygenThreshold;
                oxygenWarning.text = critical ? "AIRE CRÍTICO. Vende, gana o compra O2 ya."
                    : low ? "Te falta aire. Busca una máquina de O2." : string.Empty;
                oxygenWarning.EnableInClassList("oxygen-warning--hidden", !low);
                oxygenWarning.EnableInClassList("oxygen-warning--critical", critical);
            }
        }

        void ShowNotice(string message)
        {
            if (notice == null) return;
            notice.text = message;
            notice.RemoveFromClassList("hud-notice--hidden");
            if (noticeRoutine != null) StopCoroutine(noticeRoutine);
            noticeRoutine = StartCoroutine(HideNoticeLater());
        }

        IEnumerator HideNoticeLater()
        {
            yield return new WaitForSecondsRealtime(5f);
            notice.AddToClassList("hud-notice--hidden");
            noticeRoutine = null;
        }

        // A one-line note under the O2 bar: the tank moved and this is why. Non-blocking by design.
        void ShowOxygenEvent(int percent, string reason)
        {
            if (oxygenEvent == null || percent == 0) return;

            bool gain = percent > 0;
            oxygenEvent.text = $"{(gain ? "+" : "")}{percent}% O2 · {reason}";
            oxygenEvent.EnableInClassList("oxygen-event--gain", gain);
            oxygenEvent.EnableInClassList("oxygen-event--loss", !gain);
            oxygenEvent.RemoveFromClassList("oxygen-event--hidden");

            if (oxygenEventRoutine != null) StopCoroutine(oxygenEventRoutine);
            oxygenEventRoutine = StartCoroutine(HideOxygenEventLater());
        }

        IEnumerator HideOxygenEventLater()
        {
            yield return new WaitForSecondsRealtime(4f);
            oxygenEvent.AddToClassList("oxygen-event--hidden");
            oxygenEventRoutine = null;
        }

        void OnChipsChanged(int count)
        {
            bool earned = count > knownChips;
            knownChips = count;
            RefreshChips(count);

            // The PA only reacts to chips won at a table, never to chips spent at the minibar.
            if (!earned || chipAnnouncements.Length == 0) return;
            int index = Mathf.Clamp(count - 1, 0, chipAnnouncements.Length - 1);
            Announce(chipAnnouncements[index]);
        }

        void RefreshChips(int count)
        {
            knownChips = count;
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

        void RefreshEchoes()
        {
            if (echoBadges == null || modifiers == null) return;
            echoBadges.Clear();
            foreach (var s in modifiers.Active)
            {
                var badge = new Label($"{s.Definition.Glyph} {s.RemainingLabel}") { name = $"echo-badge-{s.Definition.Id}", tooltip = s.Definition.DisplayName };
                badge.AddToClassList("echo-badge");
                echoBadges.Add(badge);
            }
            echoesModule?.EnableInClassList("echoes-module--hidden", modifiers.Active.Count == 0);
        }

        // Timed Echoes count down on the badge; a once-per-second refresh is enough.
        float echoRefreshTimer;
        void Update()
        {
            if (modifiers == null || modifiers.Active.Count == 0) return;
            echoRefreshTimer -= Time.unscaledDeltaTime;
            if (echoRefreshTimer > 0f) return;
            echoRefreshTimer = 1f;
            RefreshEchoes();
        }

        void OnReviveUsed(float ratio) =>
            Announce($"Bombona de reserva activada: el tanque vuelve al {Mathf.RoundToInt(ratio * 100f)}%. No habrá otra.");

        // World panels (O2 machine, minibar, elevator, dialogue) sit where the banner is: hide it while one is open.
        void OnModalChanged(bool anyOpen) => ShowObjective(TutorialProgress.CurrentObjectiveOrNull);

        void ShowObjective(TutorialProgress.Objective? current)
        {
            if (objective == null) return;
            bool has = current.HasValue && !ModalTracker.IsAnyOpen;
            objective.EnableInClassList("objective--hidden", !has);
            if (!has) return;

            var o = current.Value;
            if (objectiveStep != null) objectiveStep.text = $"{o.Step}/{o.Total}";
            if (objectiveIcon != null) objectiveIcon.text = o.Icon;
            if (objectiveText != null) objectiveText.text = o.Text;
        }

        void HideAnnouncement()
        {
            if (announcementRoutine != null) StopCoroutine(announcementRoutine);
            announcementRoutine = null;
            announcement?.AddToClassList("announcement--hidden");
        }
    }
}
