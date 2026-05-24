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

        Label coinsLabel;
        Label oxygenPercent;
        VisualElement oxygenBarFill;
        VisualElement oxygenModule;

        Wallet wallet;
        OxygenTank oxygenTank;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            coinsLabel = doc.rootVisualElement.Q<Label>("coins-value");
            oxygenPercent = doc.rootVisualElement.Q<Label>("oxygen-percent");
            oxygenBarFill = doc.rootVisualElement.Q("oxygen-fill");
            oxygenModule = doc.rootVisualElement.Q("OxygenModule");
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            wallet = gm.Wallet;
            oxygenTank = gm.OxygenTank;

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
        }

        void OnDisable()
        {
            if (wallet != null)
                wallet.OnCoinsChanged -= UpdateCoins;

            if (oxygenTank != null)
                oxygenTank.OnOxygenChanged -= UpdateOxygen;
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
    }
}
