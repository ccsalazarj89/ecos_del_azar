using UnityEngine;

public class tankO2Fill : MonoBehaviour
{
    private Renderer rend;
    public int alpha;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void Update()
    {
        Color colorActual = rend.material.color;
        colorActual.a = alpha; // valor entre 0 (invisible) y 1 (visible)
        rend.material.color = colorActual;
    }
}
