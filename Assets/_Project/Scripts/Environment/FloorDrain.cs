using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.Environment
{
    /// <summary>
    /// Put one on a hub scene to make its air thinner: oxygen drains faster
    /// on upper floors, so climbing raises the stakes of every minute.
    /// Resets to 1 when a scene without this component loads.
    /// </summary>
    public class FloorDrain : MonoBehaviour
    {
        [Tooltip("Multiplier on both passive and active drain while on this floor and its tables.")]
        [SerializeField, Range(0.5f, 3f)] float drainMultiplier = 1f;

        void Start()
        {
            var tank = GameManager.Instance?.OxygenTank;
            if (tank != null) tank.FloorDrainMultiplier = drainMultiplier;
        }
    }
}
