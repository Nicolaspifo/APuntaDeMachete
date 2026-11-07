using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] private PlayerStats stats;


    [Header("Input")]
    [SerializeField] private InputActionAsset actions;

    private PlayerRuntimeStats runtimeStats; // opcional: velocidad con mejoras aplicadas
    private Rigidbody2D rb;
    private InputAction moveAction;
    private Vector2 moveInput;

    private float MoveSpeed
    {
        get
        {
            // Las mejoras viven en PlayerRuntimeStats; si no existe, se usan los datos base.
            if (runtimeStats != null && runtimeStats.MoveSpeed > 0f) return runtimeStats.MoveSpeed;
            return stats != null ? stats.moveSpeed : 5f;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        TryGetComponent(out runtimeStats);

        if (actions != null)
        {
            moveAction = actions.FindAction("Move", throwIfNotFound: true);
        }
    }

    private void OnEnable()
    {
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        moveInput = Vector2.zero;
    }

    private void Update()
    {
        if (moveAction == null) return;

        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * MoveSpeed;
    }
}
