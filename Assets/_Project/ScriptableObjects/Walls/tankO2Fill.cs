using UnityEngine;
using EcosDelAzar.Core;
public class tankO2Fill : MonoBehaviour
{
    private Renderer rend;
   // public int alpha;
    OxygenTank oxygenTank;
     
    void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

             oxygenTank = gm.OxygenTank;

            if (oxygenTank != null)
            {
                oxygenTank.OnOxygenChanged += UpdateOxygen;
                UpdateOxygen(oxygenTank.Current);
            }
        }
    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void UpdateOxygen(float o2)
    {
        Color colorActual = rend.material.color;
        colorActual.a = o2; // valor entre 0 (invisible) y 1 (visible)
        rend.material.color = colorActual;
    }
}
