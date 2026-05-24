using UnityEngine;

namespace EcosDelAzar.NPC
{
    [System.Serializable]
    public struct DialogueLine
    {
        [TextArea(2, 4)] public string text;
        public bool isReward;
    }
}
