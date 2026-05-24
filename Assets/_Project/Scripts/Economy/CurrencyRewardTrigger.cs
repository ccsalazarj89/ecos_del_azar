using UnityEngine;

namespace EcosDelAzar.Economy
{
    public class CurrencyRewardTrigger : MonoBehaviour
    {
        [SerializeField] int amount = 10;
        [SerializeField] bool destroyAfterCollect = true;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            ProgressManager.AddCurrency(amount);
            if (destroyAfterCollect) Destroy(gameObject);
        }
    }
}
