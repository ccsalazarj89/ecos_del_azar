using UnityEngine;

namespace EcosDelAzar.AI
{
    /// <summary>
    /// A dealer's personality. Create one asset per opponent type
    /// (Conservative, Aggressive, Boss...) via the Unity Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_AIBrainConfig", menuName = "Ecos del Azar/AI/Brain Config")]
    public class AIBrainConfig : ScriptableObject
    {
        [Header("Risk Profile")]
        [Tooltip("How willing the NPC is to bet big (0 = very conservative, 1 = reckless).")]
        [Range(0f, 1f)]
        public float aggressiveness = 0.3f;

        [Tooltip("How often the NPC bluffs or raises without a clear advantage.")]
        [Range(0f, 1f)]
        public float bluffFrequency = 0.15f;

        [Header("Air level (stack ratio)")]
        [Tooltip("When the dealer's stack falls below this share of its starting coins, it is 'running out of air' and plays desperate.")]
        [Range(0f, 1f)]
        public float desperateThreshold = 0.3f;

        [Tooltip("Extra aggressiveness while desperate.")]
        [Range(0f, 1f)]
        public float desperateBoost = 0.35f;
    }
}
