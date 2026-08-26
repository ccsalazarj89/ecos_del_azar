using UnityEngine;

namespace EcosDelAzar.Elevator
{
    [CreateAssetMenu(fileName = "SO_ElevatorFloor", menuName = "Ecos del Azar/Elevator/Floor Data")]
    public class ElevatorFloorData : ScriptableObject
    {
        [SerializeField] string floorId;
        [SerializeField] string floorNumber;
        [SerializeField] string displayName;
        [SerializeField] string description;
        [SerializeField] string sceneName;
        [SerializeField] bool unlockedByDefault;

        [Header("Unlock by purchase")]
        [SerializeField] bool canBePurchased = true;
        [SerializeField] int accessCost = 10;

        [Header("Unlock by house chips (boss floor)")]
        [Tooltip("Chips needed to open this floor. 0 = not gated by chips.")]
        [SerializeField] int requiredChips = 0;
        [Tooltip("How many of those chips must come from floor 2 or higher.")]
        [SerializeField] int requiredUpperFloorChips = 0;

        public string FloorId => floorId;
        public string FloorNumber => floorNumber;
        public string DisplayName => displayName;
        public string Description => description;
        public string SceneName => sceneName;
        public bool UnlockedByDefault => unlockedByDefault;
        public bool CanBePurchased => canBePurchased;
        public int AccessCost => accessCost;
        public bool RequiresChips => requiredChips > 0;
        public int RequiredChips => requiredChips;
        public int RequiredUpperFloorChips => requiredUpperFloorChips;
    }
}
