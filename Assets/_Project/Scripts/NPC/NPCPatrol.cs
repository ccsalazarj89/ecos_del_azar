using UnityEngine;

namespace EcosDelAzar.NPC
{
    /// <summary>
    /// Hace que un NPC recorra una serie de waypoints en bucle (patrulla),
    /// reproduciendo Idle/Walking a través del parámetro float "Speed" del
    /// Animator (compatible con PlayerAnimator.controller, que ya define
    /// esos estados). Pensado para NPCs con Rigidbody, igual que el jugador.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class NPCPatrol : MonoBehaviour
    {
        [Header("Ruta")]
        [Tooltip("Coloca objetos vacíos en cada punto de la ruta y arrástralos aquí, en orden.")]
        [SerializeField] Transform[] waypoints;
        [SerializeField] float waypointTolerance = 0.2f;
        [SerializeField] float waitTimeAtWaypoint = 1f;

        [Header("Movimiento")]
        [SerializeField] float speed = 2f;
        [SerializeField] float rotationSpeed = 10f;
        [Tooltip("Valor de Speed que se envía al Animator mientras camina (0-1, igual que el del jugador).")]
        [SerializeField] float animatorWalkValue = 0.3f;

        [SerializeField] Animator animator;

        Rigidbody rb;
        int currentIndex;
        float waitTimer;

        static readonly int SpeedHash = Animator.StringToHash("Speed");

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            if (animator == null) animator = GetComponentInChildren<Animator>();
            // Movement is script-driven; root motion from the walk clip (unbaked Y) would make the NPC drift upward.
            if (animator != null) animator.applyRootMotion = false;
        }

        void FixedUpdate()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (waitTimer > 0f)
            {
                waitTimer -= Time.fixedDeltaTime;
                Stop();
                return;
            }

            Transform target = waypoints[currentIndex];
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude <= waypointTolerance)
            {
                currentIndex = (currentIndex + 1) % waypoints.Length;
                waitTimer = waitTimeAtWaypoint;
                Stop();
                return;
            }

            Vector3 direction = toTarget.normalized;
            rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            animator?.SetFloat(SpeedHash, animatorWalkValue);
        }

        void Stop()
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            animator?.SetFloat(SpeedHash, 0f);
        }

        void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, waypointTolerance);

                Transform next = waypoints[(i + 1) % waypoints.Length];
                if (next != null) Gizmos.DrawLine(waypoints[i].position, next.position);
            }
        }
    }
}
