using UnityEngine;
using EcosDelAzar.Elevator;
using EcosDelAzar.Progression;

namespace EcosDelAzar.Progression
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
