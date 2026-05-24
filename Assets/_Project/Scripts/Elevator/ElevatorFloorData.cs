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
        [SerializeField] bool canBePurchased = true;
        [SerializeField] int accessCost = 10;

        public string FloorId => floorId;
        public string FloorNumber => floorNumber;
        public string DisplayName => displayName;
        public string Description => description;
        public string SceneName => sceneName;
        public bool UnlockedByDefault => unlockedByDefault;
        public bool CanBePurchased => canBePurchased;
        public int AccessCost => accessCost;
    }
}
