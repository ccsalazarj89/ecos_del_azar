using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.Elevator;
using EcosDelAzar.Economy;

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
        Label currencyLabel;
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
            currencyLabel = doc.rootVisualElement.Q<Label>("CurrencyValue");

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
            if (currencyLabel != null)
                currencyLabel.text = ProgressManager.Currency.ToString();

            floorsContainer.Clear();

            foreach (var floor in currentFloors)
            {
                if (floor == null) continue;

                bool isCurrent = ElevatorSceneLoader.IsCurrentScene(floor);
                if (isCurrent && currentFloorMode == CurrentFloorDisplayMode.Hide) continue;

                floorsContainer.Add(CreateFloorButton(floor));
            }
        }

        VisualElement CreateFloorButton(ElevatorFloorData floor)
        {
            var state = GetState(floor);
            var button = new Button { name = $"FloorButton_{floor.FloorId}" };
            button.AddToClassList("floor-button");
            button.AddToClassList(StateClass(state));

            SetupInteraction(button, floor, state);

            var numberBox = new VisualElement();
            numberBox.AddToClassList("floor-button__number-box");
            numberBox.Add(new Label(floor.FloorNumber) { name = "number" });

            var content = new VisualElement();
            content.AddToClassList("floor-button__content");
            content.Add(new Label(floor.DisplayName));
            content.Add(new Label(floor.Description));

            var action = new Label(ActionText(floor, state));
            action.AddToClassList("floor-button__action-text");

            button.Add(numberBox);
            button.Add(content);
            button.Add(action);

            return button;
        }

        FloorState GetState(ElevatorFloorData floor)
        {
            if (ElevatorSceneLoader.IsCurrentScene(floor)) return FloorState.Current;
            if (ProgressManager.IsFloorUnlocked(floor)) return FloorState.Travel;
            if (!floor.CanBePurchased) return FloorState.Locked;
            if (!ProgressManager.CanAfford(floor.AccessCost)) return FloorState.CannotAfford;
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

        void SetupInteraction(Button button, ElevatorFloorData floor, FloorState state)
        {
            switch (state)
            {
                case FloorState.Travel:
                    button.clicked += () => ElevatorSceneLoader.LoadFloor(floor);
                    break;
                case FloorState.Purchase:
                    button.clicked += () => { ProgressManager.TryPurchaseFloorAccess(floor); Refresh(); };
                    break;
                default:
                    button.SetEnabled(false);
                    break;
            }
        }

        string ActionText(ElevatorFloorData floor, FloorState state) => state switch
        {
            FloorState.Current => "ACTUAL",
            FloorState.Travel => "IR",
            FloorState.Purchase => $"COMPRAR {floor.AccessCost}",
            FloorState.CannotAfford => $"FALTAN {floor.AccessCost - ProgressManager.Currency}",
            _ => "BLOQUEADO"
        };
    }
}
