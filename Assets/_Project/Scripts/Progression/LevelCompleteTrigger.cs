using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
    [SerializeField] private ElevatorFloorData floorToUnlock;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (floorToUnlock == null)
        {
            Debug.LogWarning("No floor assigned to unlock.");
            return;
        }

        ProgressManager.UnlockFloor(floorToUnlock);
    }
}