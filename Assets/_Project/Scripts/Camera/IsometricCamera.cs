using UnityEngine;

namespace EcosDelAzar.Camera
{
    public class IsometricCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(-10f, 10f, -10f);
        [SerializeField] float smoothSpeed = 8f;
        [SerializeField] bool lookAtTarget = true;

        Vector3 velocity = Vector3.zero;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref velocity, 1f / smoothSpeed
            );

            if (lookAtTarget)
                transform.LookAt(target);
        }
    }
}
