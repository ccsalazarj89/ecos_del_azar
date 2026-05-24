using UnityEngine;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.Economy
{
    public class LevelCompleteTrigger : MonoBehaviour
    {
        [SerializeField] ElevatorFloorData floorToUnlock;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (floorToUnlock == null) return;
            ProgressManager.UnlockFloor(floorToUnlock);
        }
    }
}
