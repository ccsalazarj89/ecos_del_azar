using UnityEngine;

namespace EcosDelAzar.Environment
{
    /// <summary>
    /// Keeps the wall texture at a constant world size no matter how the piece is
    /// scaled: a 1 m and a 12 m wall show the same brick size. Meant for the
    /// WallPiece prefab (a unit cube), where each face maps UV 0..1.
    /// Runs in the editor too, so scaling in the Scene view updates instantly.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public class WallTiling : MonoBehaviour
    {
        [Tooltip("Texture repetitions per world metre.")]
        [SerializeField] float tilesPerMeter = 1f;

        static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");
        static readonly int BumpMapStId = Shader.PropertyToID("_BumpMap_ST");

        Renderer cachedRenderer;
        MaterialPropertyBlock block;
        Vector3 lastScale;

        void OnEnable() => Apply();

        void Update()
        {
            if (transform.lossyScale != lastScale) Apply();
        }

        void OnValidate() => Apply();

        void Apply()
        {
            if (cachedRenderer == null) cachedRenderer = GetComponent<Renderer>();
            block ??= new MaterialPropertyBlock();

            lastScale = transform.lossyScale;
            float length = Mathf.Max(Mathf.Abs(lastScale.x), Mathf.Abs(lastScale.z));
            var st = new Vector4(length * tilesPerMeter, Mathf.Abs(lastScale.y) * tilesPerMeter, 0f, 0f);

            cachedRenderer.GetPropertyBlock(block);
            block.SetVector(BaseMapStId, st);
            block.SetVector(BumpMapStId, st);
            cachedRenderer.SetPropertyBlock(block);
        }
    }
}
