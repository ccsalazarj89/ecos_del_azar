using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.Economy
{
    /// <summary>
    /// Demo trigger for testing currency rewards.
    /// </summary>
    public class CurrencyRewardTrigger : MonoBehaviour
    {
        [SerializeField] int amount = 10;
        [SerializeField] bool destroyAfterCollect = true;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var wallet = GameManager.Instance.Wallet;
            if (wallet == null) return;

            wallet.Add(amount);
            if (destroyAfterCollect) Destroy(gameObject);
        }
    }
}
