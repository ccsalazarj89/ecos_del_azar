using UnityEngine;

namespace EcosDelAzar.Environment
{
    /// <summary>
    /// Hides this wall while the camera's vision volume overlaps it, so geometry
    /// never blocks the isometric view of the player.
    /// Overlaps are counted: several vision colliders can be inside at once and the
    /// wall only reappears when the last one leaves.
    /// </summary>
    public class WallVisionFade : MonoBehaviour
    {
        [SerializeField] string visionTag = "Vision";
        [Tooltip("Also hide renderers on child objects.")]
        [SerializeField] bool includeChildren = true;

        Renderer[] renderers;
        int overlaps;

        void Awake()
        {
            renderers = includeChildren
                ? GetComponentsInChildren<Renderer>(true)
                : GetComponents<Renderer>();
        }

        // A disabled or reloaded wall must never be left stuck invisible.
        void OnDisable()
        {
            overlaps = 0;
            SetVisible(true);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsVision(other)) return;

            overlaps++;
            SetVisible(false);
        }

        void OnTriggerExit(Collider other)
        {
            if (!IsVision(other)) return;

            overlaps = Mathf.Max(0, overlaps - 1);
            if (overlaps == 0) SetVisible(true);
        }

        bool IsVision(Collider other) =>
            other != null && !string.IsNullOrEmpty(visionTag) && other.CompareTag(visionTag);

        void SetVisible(bool visible)
        {
            if (renderers == null) return;

            foreach (var r in renderers)
            {
                if (r != null) r.enabled = visible;
            }
        }
    }
}
