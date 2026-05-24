using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ElevatorUI : MonoBehaviour
{
    private const string HiddenClass = "elevator-hidden";

    private const string RootName = "Root";
    private const string FloorsContainerName = "FloorsContainer";
    private const string CloseButtonName = "CloseButton";
    private const string CurrencyValueName = "CurrencyValue";

    [Header("Current Floor")]
    [SerializeField]
    private CurrentFloorDisplayMode currentFloorDisplayMode =
        CurrentFloorDisplayMode.ShowDisabled;

    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement floorsContainer;
    private Button closeButton;
    private Label currencyValueLabel;

    private ElevatorFloorData[] currentFloors;

    private bool isInitialized;

    private enum FloorButtonState
    {
        Current,
        Travel,
        Purchase,
        CannotAfford,
        Locked
    }

    public enum CurrentFloorDisplayMode
    {
        ShowDisabled,
        Hide
    }

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        Initialize();
        Close();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            Debug.LogWarning("UIDocument or rootVisualElement is missing.");
            return;
        }

        root = uiDocument.rootVisualElement.Q<VisualElement>(RootName);
        floorsContainer = uiDocument.rootVisualElement.Q<VisualElement>(FloorsContainerName);
        closeButton = uiDocument.rootVisualElement.Q<Button>(CloseButtonName);
        currencyValueLabel = uiDocument.rootVisualElement.Q<Label>(CurrencyValueName);

        if (root == null)
        {
            Debug.LogWarning($"Could not find VisualElement with name '{RootName}'.");
            return;
        }

        if (floorsContainer == null)
        {
            Debug.LogWarning($"Could not find VisualElement with name '{FloorsContainerName}'.");
            return;
        }

        if (closeButton == null)
        {
            Debug.LogWarning($"Could not find Button with name '{CloseButtonName}'.");
            return;
        }

        closeButton.clicked += Close;

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.clicked -= Close;
        }
    }

    public void Open(ElevatorFloorData[] floors)
    {
        Initialize();

        if (!isInitialized)
        {
            return;
        }

        if (floors == null)
        {
            Debug.LogWarning("Elevator floors are null.");
            return;
        }

        currentFloors = floors;

        root.RemoveFromClassList(HiddenClass);

        Refresh();
    }

    public void Close()
    {
        if (root == null)
        {
            return;
        }

        if (!root.ClassListContains(HiddenClass))
        {
            root.AddToClassList(HiddenClass);
        }
    }

    private void Refresh()
    {
        UpdateCurrencyLabel();
        ClearFloorButtons();

        foreach (ElevatorFloorData floor in currentFloors)
        {
            if (floor == null)
            {
                continue;
            }

            bool isCurrentScene = ElevatorSceneLoader.IsCurrentScene(floor);

            if (isCurrentScene && currentFloorDisplayMode == CurrentFloorDisplayMode.Hide)
            {
                continue;
            }

            VisualElement floorButton = CreateFloorButton(floor);
            floorsContainer.Add(floorButton);
        }
    }

    private void UpdateCurrencyLabel()
    {
        if (currencyValueLabel == null)
        {
            return;
        }

        currencyValueLabel.text = ProgressManager.Currency.ToString();
    }

    private void ClearFloorButtons()
    {
        floorsContainer.Clear();
    }

    private VisualElement CreateFloorButton(ElevatorFloorData floor)
    {
        FloorButtonState state = GetFloorButtonState(floor);

        Button button = new Button
        {
            name = $"FloorButton_{floor.FloorId}"
        };

        button.AddToClassList("floor-button");
        button.AddToClassList(GetStateClassName(state));

        SetupButtonInteraction(button, floor, state);

        VisualElement numberBox = CreateNumberBox(floor);
        VisualElement content = CreateContent(floor);
        VisualElement actionBox = CreateActionBox(floor, state);

        button.Add(numberBox);
        button.Add(content);
        button.Add(actionBox);

        return button;
    }

    private FloorButtonState GetFloorButtonState(ElevatorFloorData floor)
    {
        if (ElevatorSceneLoader.IsCurrentScene(floor))
        {
            return FloorButtonState.Current;
        }

        if (ProgressManager.IsFloorUnlocked(floor))
        {
            return FloorButtonState.Travel;
        }

        if (!floor.CanBePurchased)
        {
            return FloorButtonState.Locked;
        }

        if (!ProgressManager.CanAfford(floor.AccessCost))
        {
            return FloorButtonState.CannotAfford;
        }

        return FloorButtonState.Purchase;
    }

    private string GetStateClassName(FloorButtonState state)
    {
        return state switch
        {
            FloorButtonState.Current => "floor-button--current",
            FloorButtonState.Travel => "floor-button--travel",
            FloorButtonState.Purchase => "floor-button--purchase",
            FloorButtonState.CannotAfford => "floor-button--cannot-afford",
            FloorButtonState.Locked => "floor-button--locked",
            _ => "floor-button--locked"
        };
    }

    private void SetupButtonInteraction(
        Button button,
        ElevatorFloorData floor,
        FloorButtonState state
    )
    {
        switch (state)
        {
            case FloorButtonState.Travel:
                button.clicked += () =>
                {
                    ElevatorSceneLoader.LoadFloor(floor);
                };
                break;

            case FloorButtonState.Purchase:
                button.clicked += () =>
                {
                    bool purchased = ProgressManager.TryPurchaseFloorAccess(floor);

                    if (!purchased)
                    {
                        Refresh();
                        return;
                    }

                    Refresh();
                };
                break;

            case FloorButtonState.Current:
            case FloorButtonState.CannotAfford:
            case FloorButtonState.Locked:
            default:
                button.SetEnabled(false);
                break;
        }
    }

    private VisualElement CreateNumberBox(ElevatorFloorData floor)
    {
        VisualElement numberBox = new VisualElement();
        numberBox.AddToClassList("floor-button__number-box");

        Label numberLabel = new Label(floor.FloorNumber);
        numberLabel.AddToClassList("floor-button__number");

        numberBox.Add(numberLabel);

        return numberBox;
    }

    private VisualElement CreateContent(ElevatorFloorData floor)
    {
        VisualElement content = new VisualElement();
        content.AddToClassList("floor-button__content");

        Label nameLabel = new Label(floor.DisplayName);
        nameLabel.AddToClassList("floor-button__name");

        Label descriptionLabel = new Label(floor.Description);
        descriptionLabel.AddToClassList("floor-button__description");

        content.Add(nameLabel);
        content.Add(descriptionLabel);

        return content;
    }

    private VisualElement CreateActionBox(
        ElevatorFloorData floor,
        FloorButtonState state
    )
    {
        VisualElement actionBox = new VisualElement();
        actionBox.AddToClassList("floor-button__action");

        Label actionLabel = new Label(GetActionText(floor, state));
        actionLabel.AddToClassList("floor-button__action-text");

        actionBox.Add(actionLabel);

        return actionBox;
    }

    private string GetActionText(
        ElevatorFloorData floor,
        FloorButtonState state
    )
    {
        return state switch
        {
            FloorButtonState.Current => "ACTUAL",
            FloorButtonState.Travel => "IR",
            FloorButtonState.Purchase => $"COMPRAR {floor.AccessCost}",
            FloorButtonState.CannotAfford => $"FALTAN {floor.AccessCost - ProgressManager.Currency}",
            FloorButtonState.Locked => "BLOQUEADO",
            _ => "BLOQUEADO"
        };
    }
}