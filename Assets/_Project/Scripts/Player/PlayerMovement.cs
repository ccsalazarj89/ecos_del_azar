using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 5f;

    private Rigidbody _rb;
    private IA_PlayerControls _inputActions;
    private Vector2 _moveInput;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inputActions = new IA_PlayerControls();
    }

    void OnEnable()
    {
        _inputActions.Player.Move.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
    }

    void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        _inputActions.Player.Move.Disable();
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
        _rb.MovePosition(_rb.position + direction * speed * Time.fixedDeltaTime);
    }
}
