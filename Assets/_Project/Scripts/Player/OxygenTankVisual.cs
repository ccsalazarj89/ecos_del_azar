using UnityEngine;
using EcosDelAzar.Core;

namespace EcosDelAzar.Player
{
    /// <summary>
    /// Drives the oxygen tank the player carries: an inner cylinder that rises inside
    /// the glass shell as the tank fills, using the same colour states as the HUD bar.
    ///
    /// Only the pivot's Y scale is ever written — the pivot sits at the base of the
    /// shell, so the fill grows upward from it and nothing is ever repositioned.
    /// Deliberately polls <see cref="OxygenTank"/> instead of subscribing to
    /// OnOxygenChanged: a visual must never be able to throw inside the tank's event
    /// chain and abort whatever raised it.
    /// </summary>
    public class OxygenTankVisual : MonoBehaviour
    {
        [Header("Fill")]
        [Tooltip("Empty transform at the base of the shell. Its Y scale is driven 0..1.")]
        [SerializeField] Transform fillPivot;
        [SerializeField] Renderer fillRenderer;
        [Tooltip("Seconds for the fill to catch up to the real level. 0 snaps.")]
        [SerializeField] float smoothing = 0.25f;

        [Header("Colors (match GameHUD oxygen bar)")]
        // Check OxygenVendingMachine USS to use the same values as HUD
        [SerializeField] Color normalColor = new Color(0f, 0.627f, 0.784f);
        [SerializeField] Color lowColor = new Color(0.902f, 0.471f, 0.118f);
        [SerializeField] Color criticalColor = new Color(1f, 0.157f, 0.157f);
        [SerializeField, Range(0f, 1f)] float lowThreshold = 0.35f;
        [SerializeField, Range(0f, 1f)] float criticalThreshold = 0.15f;

        [Header("Critical Pulse")]
        [SerializeField] float pulseSpeed = 4f;
        [SerializeField] float pulseIntensity = 2.2f;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        const float EmptyEpsilon = 0.001f;

        OxygenTank tank;
        MaterialPropertyBlock props;
        Vector3 fullScale;
        float shownRatio;
        Color shownColor;
        bool ready;

        void Awake()
        {
            if (fillPivot == null)
            {
                enabled = false;
                return;
            }

            props = new MaterialPropertyBlock();
            fullScale = fillPivot.localScale;
            shownColor = normalColor;
        }

        void LateUpdate()
        {
            if (!Bind()) return;

            float target = tank.Ratio;
            float t = smoothing > 0f ? 1f - Mathf.Exp(-Time.deltaTime / smoothing) : 1f;

            shownRatio = Mathf.Lerp(shownRatio, target, t);
            shownColor = Color.Lerp(shownColor, ColorFor(target), t);

            Apply(Mathf.Clamp01(shownRatio));
        }

        bool Bind()
        {
            if (tank != null) return true;

            // GameManager may spawn after this object in some scenes.
            tank = GameManager.Instance?.OxygenTank;
            if (tank == null) return false;

            shownRatio = tank.Ratio;
            shownColor = ColorFor(shownRatio);
            ready = true;
            return true;
        }

        void Apply(float ratio)
        {
            var scale = fullScale;
            scale.y = fullScale.y * ratio;
            fillPivot.localScale = scale;

            if (fillRenderer == null) return;

            bool visible = ratio > EmptyEpsilon;
            if (fillRenderer.enabled != visible) fillRenderer.enabled = visible;
            if (!visible) return;

            float emission = ratio <= criticalThreshold
                ? Mathf.Lerp(1f, pulseIntensity, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f)
                : 1f;

            fillRenderer.GetPropertyBlock(props);
            props.SetColor(BaseColorId, shownColor);
            props.SetColor(EmissionColorId, shownColor * emission);
            fillRenderer.SetPropertyBlock(props);
        }

        // Discrete states, so the tank reads the same colour as the HUD bar at any level.
        Color ColorFor(float ratio)
        {
            if (ratio <= criticalThreshold) return criticalColor;
            if (ratio <= lowThreshold) return lowColor;
            return normalColor;
        }

        /// <summary>True once the visual has found the tank and is tracking it.</summary>
        public bool IsTracking => ready;
    }
}
