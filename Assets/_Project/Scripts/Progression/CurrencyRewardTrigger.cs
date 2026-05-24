using UnityEngine;

public class CurrencyRewardTrigger : MonoBehaviour
{
    [SerializeField] private int amount = 10;
    [SerializeField] private bool destroyAfterCollect = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        ProgressManager.AddCurrency(amount);

        if (destroyAfterCollect)
        {
            Destroy(gameObject);
        }
    }
}