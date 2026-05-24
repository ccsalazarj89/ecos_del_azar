using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_ElevatorFloor_NewFloor",
    menuName = "Ecos del Azar/Elevator/Floor Data"
)]
public class ElevatorFloorData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string floorId;
    [SerializeField] private string floorNumber;
    [SerializeField] private string displayName;
    [SerializeField] private string description;

    [Header("Scene")]
    [SerializeField] private string sceneName;

    [Header("Progression")]
    [SerializeField] private bool unlockedByDefault;

    [Header("Access Purchase")]
    [SerializeField] private bool canBePurchased = true;
    [SerializeField] private int accessCost = 10;

    public string FloorId => floorId;
    public string FloorNumber => floorNumber;
    public string DisplayName => displayName;
    public string Description => description;
    public string SceneName => sceneName;
    public bool UnlockedByDefault => unlockedByDefault;
    public bool CanBePurchased => canBePurchased;
    public int AccessCost => accessCost;
}