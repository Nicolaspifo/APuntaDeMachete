using System;
using UnityEngine;

/// <summary>
/// Ataque cuerpo a cuerpo con anticipacion. Estados:
/// Idle → Windup (se detiene y avisa) → golpe → Recover (quieto, vulnerable) → Idle.
/// Un enemigo solo empieza a cargar si consigue una ficha de <see cref="AttackTokens"/>.
/// </summary>
public class EnemyContactDamage : MonoBehaviour
{
    private enum State { Idle, Windup, Recover }

    [SerializeField, Min(0f)] private float damage = 5f;
    [SerializeField, Min(0f)] private float range = 1.2f;
    [SerializeField, Min(0.1f)] private float cooldown = 1f;

    [Header("Anticipacion (windup)")]
    [Tooltip("Segundos quieto avisando antes de golpear. Es el tiempo que tiene el jugador para salir.")]
    [SerializeField, Min(0f)] private float windup = 0.3f;
    [Tooltip("Segundos quieto tras golpear. Ventana para contraatacar o alejarse.")]
    [SerializeField, Min(0f)] private float recover = 0.3f;
    [Tooltip("Fraccion del rango visible que realmente hace dano. <1 = un poco generoso con el jugador.")]
    [SerializeField, Range(0.5f, 1f)] private float damageRadiusScale = 0.9f;

    [Header("Fichas de ataque")]
    [Tooltip("Cuantos enemigos pueden estar cargando/golpeando a la vez (todos los tipos).")]
    [SerializeField, Min(1)] private int maxConcurrentAttackers = 3;

    /// <summary>Empieza la carga. Parametro: duracion del windup en segundos.</summary>
    public event Action<float> WindupStarted;
    /// <summary>Se dispara cuando el golpe conecta.</summary>
    public event Action Attacked;

    public float Range => range;
    public float Windup => windup;
    /// <summary>True mientras carga o se recupera: el enemigo no debe moverse.</summary>
    public bool IsBusy => state != State.Idle;

    private const float RetargetInterval = 0.5f;

    private Transform target;
    private IDamageable damageable;
    private State state = State.Idle;
    private float stateEndTime;
    private float nextHitTime;
    private float nextRetarget;

    private void Start() => FindTarget();

    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        target = player.transform;
        player.TryGetComponent(out damageable);
    }

    private void Update()
    {
        if (target == null || damageable == null)
        {
            if (Time.time >= nextRetarget)
            {
                nextRetarget = Time.time + RetargetInterval;
                FindTarget();
            }
            return;
        }

        if (damageable.IsDead)
        {
            Stop();
            return;
        }

        switch (state)
        {
            case State.Idle:
                TryStartWindup();
                break;

            case State.Windup:
                if (Time.time >= stateEndTime) Strike();
                break;

            case State.Recover:
                if (Time.time >= stateEndTime) Finish();
                break;
        }
    }

    private void TryStartWindup()
    {
        if (Time.time < nextHitTime || !InAttackRange()) return;
        if (!AttackTokens.TryAcquire(this, maxConcurrentAttackers)) return;

        state = State.Windup;
        stateEndTime = Time.time + windup;
        WindupStarted?.Invoke(windup);
    }

    private void Strike()
    {
        // El golpe solo cuenta si el jugador sigue dentro; si salio, el enemigo falla igualmente.
        if (InStrikeRange() && !damageable.IsDead)
        {
            damageable.TakeDamage(damage, gameObject);
            Attacked?.Invoke();
        }

        nextHitTime = Time.time + cooldown;
        state = State.Recover;
        stateEndTime = Time.time + recover;
        AttackTokens.Release(this);
    }

    private void Finish()
    {
        state = State.Idle;
    }

    private void Stop()
    {
        state = State.Idle;
        AttackTokens.Release(this);
    }

    private bool InAttackRange() => DistanceSqToTarget() <= range * range;

    private bool InStrikeRange()
    {
        float r = range * damageRadiusScale;
        return DistanceSqToTarget() <= r * r;
    }

    private float DistanceSqToTarget() =>
        ((Vector2)target.position - (Vector2)transform.position).sqrMagnitude;

    private void OnDisable() => AttackTokens.Release(this);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
