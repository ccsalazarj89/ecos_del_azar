using UnityEngine;

public class DiceView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Dice Faces")]
    [SerializeField] private Sprite[] faceSprites = new Sprite[6];

    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float maxScaleMultiplier = 1.15f;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetFace(int value)
    {
        if (value < 1 || value > 6)
        {
            Debug.LogError($"Invalid dice value: {value}", this);
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("Missing SpriteRenderer reference.", this);
            return;
        }

        if (faceSprites == null || faceSprites.Length < 6 || faceSprites[value - 1] == null)
        {
            Debug.LogError($"Missing sprite for dice face {value}.", this);
            return;
        }

        spriteRenderer.sprite = faceSprites[value - 1];
    }

    public void SetRandomFace()
    {
        SetFace(Random.Range(1, 7));
    }

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