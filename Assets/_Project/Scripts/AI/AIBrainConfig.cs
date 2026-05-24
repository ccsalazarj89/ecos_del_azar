using UnityEngine;

namespace EcosDelAzar.AI
{
    /// <summary>
    /// Config for AI behavior.
    /// Create different presets (Conservative, Aggressive, Boss) via the Unity Inspector.
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

        [Tooltip("Win probability below which the NPC considers folding.")]
        [Range(0f, 1f)]
        public float foldThreshold = 0.2f;

        [Header("Betting Limits")]
        [Tooltip("Maximum multiplier the NPC applies to the opponent's bet when raising.")]
        [Min(1f)]
        public float maxRaiseMultiplier = 3f;

        [Tooltip("Chip ratio below which the NPC enters 'desperate' mode.")]
        [Range(0f, 1f)]
        public float desperateThreshold = 0.2f;
    }
}
