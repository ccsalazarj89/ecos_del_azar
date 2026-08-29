using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EcosDelAzar.Environment
{
    /// <summary>
    /// Put this on the parent of all walls of a level. Every child renderer whose
    /// bounds sit between the camera and the player fades to a translucent ghost,
    /// so the isometric view is never blocked. Works with whole walls or pieces:
    /// no per-wall setup, no trigger volumes. Colliders are never touched, so
    /// faded walls still block the player. Anything under a neverFade transform is
    /// skipped and stays solid.
    /// Materials must support the URP Lit surface switch (URP/Lit, Simple Lit...).
    /// Others (e.g. Synty shader graphs) fall back to hiding the renderer.
    /// </summary>
    public class WallOcclusionFader : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("Defaults to the object tagged Player.")]
        [SerializeField] Transform player;
        [Tooltip("Defaults to Camera.main.")]
        [SerializeField] UnityEngine.Camera viewCamera;

        [Header("Fade")]
        [Range(0f, 1f)] [SerializeField] float hiddenAlpha = 0.25f;
        [SerializeField] float fadeSpeed = 6f;
        [Tooltip("Extra wall thickness used for the occlusion test, so walls fade slightly before they cover the player.")]
        [SerializeField] float boundsPadding = 0.5f;
        [Tooltip("Height above the player's pivot that must stay visible.")]
        [SerializeField] float playerHeight = 1.8f;

        [Header("Exceptions")]
        [Tooltip("Renderers under these transforms never fade: props, signs, doors, feature walls.")]
        [SerializeField] Transform[] neverFade;

        [Header("Collision")]
        [Tooltip("Adds a BoxCollider to any wall renderer that has no collider, so walls always bound the map.")]
        [SerializeField] bool ensureColliders = true;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int SurfaceId = Shader.PropertyToID("_Surface");

        class Wall
        {
            public Renderer renderer;
            public Material[] opaque;
            public Material[] translucent;
            public bool supportsFade;
            public float alpha = 1f;
            public bool usingTranslucent;
        }

        readonly List<Wall> walls = new();
        readonly Dictionary<Material, Material> translucentCache = new();
        bool warnedUnsupported;

        void Awake()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                // Props, signs and doors marked as exceptions keep their solid look.
                if (IsException(r.transform))
                {
                    if (ensureColliders && r.GetComponent<Collider>() == null)
                        r.gameObject.AddComponent<BoxCollider>();
                    continue;
                }

                var opaque = r.sharedMaterials;
                var translucent = new Material[opaque.Length];
                bool supported = opaque.Length > 0;

                for (int i = 0; i < opaque.Length; i++)
                {
                    translucent[i] = GetTranslucent(opaque[i]);
                    if (translucent[i] == null) supported = false;
                }

                if (!supported && !warnedUnsupported)
                {
                    warnedUnsupported = true;
                    Debug.LogWarning($"[WallOcclusionFader] '{r.name}' uses a material without URP Lit surface options; it will be hidden instead of faded. Use the WallPiece prefab / M_Wall material for translucent walls.", r);
                }

                if (ensureColliders && r.GetComponent<Collider>() == null)
                    r.gameObject.AddComponent<BoxCollider>();

                walls.Add(new Wall { renderer = r, opaque = opaque, translucent = translucent, supportsFade = supported });
            }
        }

        bool IsException(Transform t)
        {
            if (neverFade != null)
                foreach (var root in neverFade)
                    if (root != null && t.IsChildOf(root)) return true;

            return false;
        }

        void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (viewCamera == null) viewCamera = UnityEngine.Camera.main;
        }

        void OnDestroy()
        {
            foreach (var m in translucentCache.Values)
                if (m != null) Destroy(m);
        }

        void LateUpdate()
        {
            if (player == null || viewCamera == null) return;

            Vector3 camPos = viewCamera.transform.position;
            Vector3 feet = player.position;
            Vector3 head = feet + Vector3.up * playerHeight;

            foreach (var wall in walls)
            {
                if (wall.renderer == null) continue;

                Bounds b = wall.renderer.bounds;
                bool blocking = Occludes(b, camPos, feet) || Occludes(b, camPos, head);

                if (!wall.supportsFade)
                {
                    wall.renderer.enabled = !blocking;
                    continue;
                }

                float target = blocking ? hiddenAlpha : 1f;
                wall.alpha = Mathf.MoveTowards(wall.alpha, target, fadeSpeed * Time.deltaTime);
                Apply(wall);
            }
        }

        bool Occludes(Bounds bounds, Vector3 from, Vector3 to)
        {
            bounds.Expand(boundsPadding);
            Vector3 delta = to - from;
            var ray = new Ray(from, delta.normalized);
            return bounds.IntersectRay(ray, out float distance) && distance < delta.magnitude;
        }

        void Apply(Wall wall)
        {
            bool translucent = wall.alpha < 0.999f;

            if (translucent != wall.usingTranslucent)
            {
                wall.renderer.sharedMaterials = translucent ? wall.translucent : wall.opaque;
                wall.usingTranslucent = translucent;
            }

            if (!translucent) return;

            foreach (var m in wall.translucent)
            {
                Color c = m.GetColor(BaseColorId);
                c.a = wall.alpha;
                m.SetColor(BaseColorId, c);
            }
        }

        Material GetTranslucent(Material source)
        {
            if (source == null) return null;
            if (translucentCache.TryGetValue(source, out var cached)) return cached;

            if (!source.HasProperty(SurfaceId) || !source.HasProperty(BaseColorId))
                return null;

            var m = new Material(source) { name = source.name + " (Translucent)" };
            MakeTransparent(m);
            translucentCache[source] = m;
            return m;
        }

        // Runtime equivalent of switching Surface Type to Transparent / Alpha in the URP Lit inspector.
        static void MakeTransparent(Material m)
        {
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.DisableKeyword("_ALPHATEST_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetShaderPassEnabled("DepthOnly", false);
            m.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
