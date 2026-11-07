using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 4f;
    [SerializeField, Min(0f)] private float stopDistance = 1.3f;

    [Header("Rodeo")]
    [Tooltip("Cada enemigo elige un angulo al aparecer y se coloca en ese lado del jugador, asi no forman fila.")]
    [SerializeField] private bool flank = true;

    [Header("Separacion entre enemigos")]
    [Tooltip("Evita que los enemigos se apilen. Debe ser mayor que el tamaño del sprite.")]
    [SerializeField, Min(0f)] private float separationRadius = 1.3f;
    [SerializeField, Min(0f)] private float separationWeight = 2f;
    [Tooltip("Capas con las que se separa. Si esta en Nothing usa la capa de este mismo enemigo.")]
    [SerializeField] private LayerMask separationLayers;

    private const float RetargetInterval = 0.5f;

    private Rigidbody2D body;
    private Transform target;
    private EnemyContactDamage contact;
    private ContactFilter2D separationFilter;
    private readonly Collider2D[] neighbours = new Collider2D[16];
    private float nextRetarget;
    private float flankAngle;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        contact = GetComponent<EnemyContactDamage>();
        flankAngle = Random.Range(0f, Mathf.PI * 2f);

        LayerMask mask = separationLayers.value != 0 ? separationLayers : (LayerMask)(1 << gameObject.layer);
        separationFilter = new ContactFilter2D { useLayerMask = true, layerMask = mask, useTriggers = false };
    }

    private void Start() => FindTarget();

    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            // El jugador pudo aparecer despues (o reinstanciarse): reintenta sin hacerlo cada frame.
            if (Time.time >= nextRetarget)
            {
                nextRetarget = Time.time + RetargetInterval;
                FindTarget();
            }
            if (target == null) return;
        }

        // Mientras carga o se recupera de un golpe, el enemigo se queda quieto (es la oportunidad del jugador).
        if (contact != null && contact.IsBusy) return;

        float attackDistance = contact != null ? contact.Range * 0.9f : stopDistance;
        Vector2 goal = flank ? RingPoint(attackDistance) : (Vector2)target.position;
        Vector2 toGoal = goal - body.position;
        Vector2 move = Vector2.zero;

        if (toGoal.sqrMagnitude > 0.01f)
            move = toGoal.normalized;

        move += Separation() * separationWeight;

        if (move.sqrMagnitude < 0.0001f) return;

        body.MovePosition(body.position + move.normalized * (speed * Time.fixedDeltaTime));
    }

    /// <summary>Punto en el anillo alrededor del jugador, en el angulo asignado a este enemigo.</summary>
    private Vector2 RingPoint(float radius)
    {
        Vector2 dir = new Vector2(Mathf.Cos(flankAngle), Mathf.Sin(flankAngle));
        return (Vector2)target.position + dir * radius;
    }

    /// <summary>Empuje que aleja de los vecinos cercanos; mas fuerte cuanto mas cerca.</summary>
    private Vector2 Separation()
    {
        if (separationRadius <= 0f) return Vector2.zero;

        int count = Physics2D.OverlapCircle(body.position, separationRadius, separationFilter, neighbours);
        Vector2 push = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Collider2D other = neighbours[i];
            if (other.attachedRigidbody == body) continue;

            Vector2 away = body.position - (Vector2)other.transform.position;
            float distance = away.magnitude;

            if (distance < 0.0001f)
                away = Random.insideUnitCircle; // exactamente encima: elige una direccion al azar
            else
                push += away / distance * (1f - distance / separationRadius);
        }

        return push;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
