using UnityEngine;

namespace EcosDelAzar.MiniGames
{
    public class DiceView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] faceSprites = new Sprite[6];
        [SerializeField] float rotationSpeed = 720f;
        [SerializeField] float maxScaleMultiplier = 1.15f;

        Vector3 originalScale;
        Quaternion originalRotation;

        void Awake()
        {
            originalScale = transform.localScale;
            originalRotation = transform.rotation;
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetFace(int value)
        {
            if (value < 1 || value > 6) return;
            if (spriteRenderer == null || faceSprites[value - 1] == null) return;
            spriteRenderer.sprite = faceSprites[value - 1];
        }

        public void SetRandomFace() => SetFace(Random.Range(1, 7));

        public void AnimateRolling(float normalizedTime)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            float scale = Mathf.Lerp(maxScaleMultiplier, 1f, normalizedTime);
            transform.localScale = originalScale * scale;
        }

        public void ResetVisuals()
        {
            transform.rotation = originalRotation;
            transform.localScale = originalScale;
        }
    }
}
