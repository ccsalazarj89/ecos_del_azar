using UnityEngine;
using UnityEngine.InputSystem;

namespace EcosDelAzar.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float speed = 5f;
        [SerializeField] float rotationSpeed = 10f;

        Rigidbody rb;
        Animator animator;
        IA_PlayerControls inputActions;
        Vector2 moveInput;
        Transform cameraTransform;

        static readonly int SpeedHash = Animator.StringToHash("Speed");

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            inputActions = new IA_PlayerControls();
            animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            cameraTransform = UnityEngine.Camera.main.transform;
        }

        void OnEnable()
        {
            if (inputActions == null) inputActions = new IA_PlayerControls();
            inputActions.Player.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
        }

        void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Move.performed -= OnMove;
                inputActions.Player.Move.canceled -= OnMove;
                inputActions.Player.Disable();
            }
        }

        void OnDestroy() => inputActions?.Dispose();

        void FixedUpdate()
        {
            // Actualizar animación con la magnitud del input (0 = idle, ~1 = moviéndose)
            animator?.SetFloat(SpeedHash, moveInput.magnitude);

            if (moveInput == Vector2.zero)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                return;
            }

            // Get camera-relative directions projected on ground plane
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 direction = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);

            // Rotar el personaje hacia la dirección de movimiento
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    }
}
