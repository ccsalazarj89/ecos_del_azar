using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.Core.Echoes
{
    /// <summary>Every Echo that can appear in a run. Referenced by the GameManager prefab.</summary>
    [CreateAssetMenu(fileName = "SO_EcoCatalog", menuName = "Ecos del Azar/Echo Catalog")]
    public class EcoCatalog : ScriptableObject
    {
        [SerializeField] EcoDefinition[] echoes;

        public IReadOnlyList<EcoDefinition> All => echoes;

        public EcoDefinition Find(string id)
        {
            foreach (var e in echoes)
                if (e != null && e.Id == id) return e;
            return null;
        }
    }
}
