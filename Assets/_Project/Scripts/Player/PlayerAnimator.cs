using UnityEngine;

/// <summary>
/// Alimenta el Animator del jugador con la direccion de movimiento y los ataques.
///
/// La hoja tiene 4 direcciones diagonales:
///   0 = abajo-izquierda
///   1 = abajo-derecha
///   2 = arriba-izquierda
///   3 = arriba-derecha
///
/// Parametros esperados en el Animator Controller:
///   MoveX (float), MoveY (float): direccion diagonal (-1 o 1 en cada eje).
///   Speed (float): 0 = quieto, 1 = caminando.
///   Direction (int): 0 = abajo-izquierda, 1 = abajo-derecha, 2 = arriba-izquierda, 3 = arriba-derecha.
///   Attack (trigger): se dispara en cada golpe del machete.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Referencias (se buscan solas si se dejan vacias)")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private MacheteAttack attack;

    [Tooltip("Magnitud minima del input para cambiar ese eje. Evita cambios por ruido del joystick.")]
    [SerializeField, Range(0f, 0.9f)] private float axisThreshold = 0.25f;

    private Animator animator;

    // Direccion diagonal actual. Se conserva cuando el jugador se detiene.
    private Vector2 facing = new Vector2(-1f, -1f); // abajo-izquierda

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DirectionHash = Animator.StringToHash("Direction");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (attack == null) attack = GetComponent<MacheteAttack>();
    }

    private void Start()
    {
        // Inicializa los parametros aunque el jugador no se haya movido todavia.
        SetFacing(facing);
    }

    private void OnEnable()
    {
        if (attack != null) attack.Attacked += OnAttacked;
    }

    private void OnDisable()
    {
        if (attack != null) attack.Attacked -= OnAttacked;
    }

    private void Update()
    {
        if (movement != null && movement.IsMoving)
            SetFacing(movement.MoveInput);

        animator.SetFloat(SpeedHash, movement != null && movement.IsMoving ? 1f : 0f);
    }

    private void OnAttacked(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
            SetFacing(direction);

        animator.SetTrigger(AttackHash);
    }

    /// <summary>
    /// Actualiza solo los ejes que el input esta usando. Asi, al moverse en
    /// horizontal o vertical, conserva el otro eje y no salta entre las dos
    /// diagonales de arriba o de abajo.
    /// </summary>
    private void SetFacing(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        if (Mathf.Abs(direction.x) >= axisThreshold)
            facing.x = direction.x > 0f ? 1f : -1f;

        if (Mathf.Abs(direction.y) >= axisThreshold)
            facing.y = direction.y > 0f ? 1f : -1f;

        animator.SetInteger(DirectionHash, DirectionFromFacing());
        animator.SetFloat(MoveXHash, facing.x);
        animator.SetFloat(MoveYHash, facing.y);
    }

    private int DirectionFromFacing()
    {
        if (facing.x < 0f)
            return facing.y < 0f ? 0 : 2; // abajo-izquierda / arriba-izquierda
        return facing.y < 0f ? 1 : 3;     // abajo-derecha / arriba-derecha
    }
}
