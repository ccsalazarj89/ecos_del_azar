using UnityEngine;

public class IsometricCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Configuración")]
    public Vector3 offset = new Vector3(-10f, 10f, -10f);
    public float smoothSpeed = 8f;
    public bool lookAtTarget = true;

    private Vector3 _velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // SmoothDamp es más suave que Lerp, evita el "lag" visual
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _velocity,
            1f / smoothSpeed
        );

        if (lookAtTarget)
            transform.LookAt(target);
    }
}
