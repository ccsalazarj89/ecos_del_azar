using System;
using System.Collections;
using UnityEngine;
using EcosDelAzar.Elevator;
using EcosDelAzar.MiniGames;

namespace EcosDelAzar.Player
{
    /// <summary>
    /// Walks the player to a table's seat and sits them before the minigame
    /// loads; when the hub comes back, spawns them seated at the same chair and
    /// stands them up. Movement is disabled while any of this runs.
    /// Uses the Animator "Seated" bool (Sitting state in PlayerAnimator).
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerSeating : MonoBehaviour
    {
        [SerializeField] float walkToSeatSpeed = 2.5f;
        [SerializeField] float turnSpeed = 10f;
        [SerializeField] float sitSettleTime = 0.9f;
        [SerializeField] float standUpTime = 0.7f;
        [Tooltip("Animator Speed value sent while walking to the chair.")]
        [SerializeField] float walkAnimValue = 0.3f;

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int SeatedHash = Animator.StringToHash("Seated");

        PlayerMovement movement;
        Rigidbody rb;
        Animator animator;

        public bool IsBusy { get; private set; }

        void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            rb = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            // Coming back from a table: appear on its chair and stand up.
            string tableId = ElevatorSceneLoader.ConsumeReturnedTableId();
            if (string.IsNullOrEmpty(tableId)) return;

            var table = FindTable(tableId);
            if (table?.SeatAnchor != null)
                StartCoroutine(StandUpRoutine(table.SeatAnchor, table.StandAnchor));
        }

        public void SitAt(Transform seat, Action onSeated)
        {
            if (IsBusy) return;
            StartCoroutine(SitRoutine(seat, onSeated));
        }

        IEnumerator SitRoutine(Transform seat, Action onSeated)
        {
            BeginControl();

            if (seat != null)
            {
                // Walk on the ground plane to the chair, then face the way the chair faces.
                yield return WalkTo(seat.position);
                Quaternion seatRotation = Quaternion.Euler(0f, seat.eulerAngles.y, 0f);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * turnSpeed;
                    transform.rotation = Quaternion.Slerp(transform.rotation, seatRotation, t);
                    yield return null;
                }
            }

            animator?.SetFloat(SpeedHash, 0f);
            animator?.SetBool(SeatedHash, true);
            yield return new WaitForSeconds(sitSettleTime);

            IsBusy = false;
            onSeated?.Invoke();
        }

        IEnumerator StandUpRoutine(Transform seat, Transform stand)
        {
            BeginControl();

            transform.SetPositionAndRotation(
                new Vector3(seat.position.x, transform.position.y, seat.position.z),
                Quaternion.Euler(0f, seat.eulerAngles.y, 0f));

            if (animator != null)
            {
                animator.SetBool(SeatedHash, true);
                animator.Play("Sitting", 0, 0f);
            }

            yield return new WaitForSeconds(0.3f);
            animator?.SetBool(SeatedHash, false);
            yield return new WaitForSeconds(standUpTime);

            if (stand != null)
                yield return WalkTo(stand.position);

            EndControl();
        }

        // Straight-line walk on the ground plane with the walking animation; no physics.
        IEnumerator WalkTo(Vector3 worldPosition)
        {
            animator?.SetFloat(SpeedHash, walkAnimValue);
            Vector3 target = new Vector3(worldPosition.x, transform.position.y, worldPosition.z);

            while ((transform.position - target).sqrMagnitude > 0.01f)
            {
                Vector3 dir = (target - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, target, walkToSeatSpeed * Time.deltaTime);
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
            animator?.SetFloat(SpeedHash, 0f);
        }

        void BeginControl()
        {
            IsBusy = true;
            movement.enabled = false;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        void EndControl()
        {
            rb.isKinematic = false;
            movement.enabled = true;
            IsBusy = false;
        }

        static MinigameEntryTrigger FindTable(string tableId)
        {
            foreach (var t in FindObjectsByType<MinigameEntryTrigger>(FindObjectsSortMode.None))
                if (t.TableId == tableId) return t;
            return null;
        }
    }
}
