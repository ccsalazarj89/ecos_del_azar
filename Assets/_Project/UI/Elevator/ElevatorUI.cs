using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.Core;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ElevatorUI : MonoBehaviour
    {
        [SerializeField] CurrentFloorDisplayMode currentFloorMode = CurrentFloorDisplayMode.ShowDisabled;

        const string HiddenClass = "elevator-hidden";

        VisualElement root;
        VisualElement floorsContainer;
        Button closeButton;
        ElevatorFloorData[] currentFloors;
        bool initialized;

        public enum CurrentFloorDisplayMode { ShowDisabled, Hide }

        enum FloorState { Current, Travel, Purchase, CannotAfford, Locked }

        void Awake()
        {
            Initialize();
            Close();
        }

        void OnEnable() => Initialize();

        void Initialize()
        {
            if (initialized) return;

            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            root = doc.rootVisualElement.Q("Root");
            floorsContainer = doc.rootVisualElement.Q("FloorsContainer");
            closeButton = doc.rootVisualElement.Q<Button>("CloseButton");

            if (root == null || floorsContainer == null || closeButton == null) return;

            closeButton.clicked += Close;
            initialized = true;
        }

        void OnDestroy()
        {
            if (closeButton != null) closeButton.clicked -= Close;
        }

        public void Open(ElevatorFloorData[] floors)
        {
            Initialize();
            if (!initialized || floors == null) return;

            currentFloors = floors;
            root.RemoveFromClassList(HiddenClass);
            Refresh();
        }

        public void Close()
        {
            if (root == null) return;
            if (!root.ClassListContains(HiddenClass))
                root.AddToClassList(HiddenClass);
        }

        void Refresh()
        {
            var wallet = GameManager.Instance?.Wallet;
            var floorProgress = GameManager.Instance?.FloorProgress;

            floorsContainer.Clear();

            foreach (var floor in currentFloors)
            {
                if (floor == null) continue;

                bool isCurrent = ElevatorSceneLoader.IsCurrentScene(floor);
                if (isCurrent && currentFloorMode == CurrentFloorDisplayMode.Hide) continue;

                floorsContainer.Add(CreateFloorButton(floor, wallet, floorProgress));
            }
        }

        VisualElement CreateFloorButton(ElevatorFloorData floor, Wallet wallet, FloorProgress floorProgress)
        {
            var state = GetState(floor, wallet, floorProgress);
            var button = new Button { name = $"FloorButton_{floor.FloorId}" };
            button.AddToClassList("floor-button");
            button.AddToClassList(StateClass(state));

            SetupInteraction(button, floor, wallet, floorProgress, state);

            var numberBox = new VisualElement();
            numberBox.AddToClassList("floor-button__number-box");
            numberBox.Add(new Label(floor.FloorNumber) { name = "number" });

            var content = new VisualElement();
            content.AddToClassList("floor-button__content");

            var nameLabel = new Label(floor.DisplayName);
            nameLabel.AddToClassList("floor-button__name");

            var descLabel = new Label(floor.Description);
            descLabel.AddToClassList("floor-button__description");

            content.Add(nameLabel);
            content.Add(descLabel);

            var action = new Label(ActionText(floor, state, wallet));
            action.AddToClassList("floor-button__action-text");

            button.Add(numberBox);
            button.Add(content);
            button.Add(action);

            return button;
        }

        FloorState GetState(ElevatorFloorData floor, Wallet wallet, FloorProgress floorProgress)
        {
            if (ElevatorSceneLoader.IsCurrentScene(floor)) return FloorState.Current;
            if (floorProgress != null && floorProgress.IsUnlocked(floor)) return FloorState.Travel;
            if (!floor.CanBePurchased) return FloorState.Locked;
            if (wallet == null || !wallet.CanAfford(floor.AccessCost)) return FloorState.CannotAfford;
            return FloorState.Purchase;
        }

        string StateClass(FloorState state) => state switch
        {
            FloorState.Current => "floor-button--current",
            FloorState.Travel => "floor-button--travel",
            FloorState.Purchase => "floor-button--purchase",
            FloorState.CannotAfford => "floor-button--cannot-afford",
            _ => "floor-button--locked"
        };

        void SetupInteraction(Button button, ElevatorFloorData floor, Wallet wallet, FloorProgress floorProgress, FloorState state)
        {
            switch (state)
            {
                case FloorState.Travel:
                    button.clicked += () => ElevatorSceneLoader.LoadFloor(floor);
                    break;
                case FloorState.Purchase:
                    button.clicked += () =>
                    {
                        if (floorProgress != null)
                        {
                            floorProgress.TryPurchaseAccess(floor, wallet);
                        }
                        Refresh();
                    };
                    break;
                default:
                    button.SetEnabled(false);
                    break;
            }
        }

        string ActionText(ElevatorFloorData floor, FloorState state, Wallet wallet) => state switch
        {
            FloorState.Current => "ACTUAL",
            FloorState.Travel => "IR",
            FloorState.Purchase => $"COMPRAR {floor.AccessCost}",
            FloorState.CannotAfford => $"FALTAN {floor.AccessCost - (wallet?.Coins ?? 0)}",
            _ => "BLOQUEADO"
        };
    }
}
