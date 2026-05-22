using UnityEngine;
using UnityEngine.UIElements;

public class ElevatorUI : MonoBehaviour
{

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
    }

}
