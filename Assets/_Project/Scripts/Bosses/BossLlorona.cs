using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossLlorona : MonoBehaviour
{
    [Header("Fases (% de vida)")]
    [SerializeField, Range(0f, 1f)] float phase2Threshold = 0.6f;
    [SerializeField, Range(0f, 1f)] float phase3Threshold = 0.3f;

    [Header("Movimiento")]
    [SerializeField] float[] speedPerPhase = { 1.5f, 2.2f, 3f };
    [SerializeField] float stopDistance = 1.2f;
    [SerializeField] float phaseChangePause = 1f;

    public int Phase { get; private set; } = 1;
    public bool IsDead { get; private set; }
    public bool MovementLocked { get; set; }

    public event Action<int> OnPhaseChanged;
    public event Action OnDefeated;

    Health health;
    Rigidbody2D rb;
    EnemyContactDamage contact;
    Transform target;
    float pauseUntil;
    float nextRetarget;

    void Awake()
    {
        health = GetComponent<Health>();
        contact = GetComponent<EnemyContactDamage>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Igual que EnemyChase: cinemático, se mueve solo con MovePosition.
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    void Start() => FindTarget();

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    void Update()
    {
        if (IsDead) return;

        float max = health.Max;
        if (max <= 0f) return;

        float pct = health.Current / max;
        if (pct <= 0f)
        {
            Die();
            return;
        }

        int wanted = pct <= phase3Threshold ? 3 : pct <= phase2Threshold ? 2 : 1;
        if (wanted > Phase) SetPhase(wanted);
    }

    void FixedUpdate()
    {
        if (IsDead) return;

        if (target == null)
        {
            if (Time.time >= nextRetarget)
            {
                nextRetarget = Time.time + 0.5f;
                FindTarget();
            }
            if (target == null) return;
        }

        if (MovementLocked || Time.time < pauseUntil) return;
        if (contact != null && contact.IsBusy) return;   // cargando o recuperándose de un golpe

        float attackDistance = contact != null ? contact.Range * 0.9f : stopDistance;

        Vector2 pos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 toTarget = (Vector2)target.position - pos;
        if (toTarget.magnitude <= attackDistance) return;

        float speed = speedPerPhase[Mathf.Clamp(Phase - 1, 0, speedPerPhase.Length - 1)];
        Vector2 next = pos + toTarget.normalized * speed * Time.fixedDeltaTime;

        if (rb != null) rb.MovePosition(next);
        else transform.position = next;
    }

    void SetPhase(int p)
    {
        Phase = p;
        pauseUntil = Time.time + phaseChangePause;
        OnPhaseChanged?.Invoke(p);
        Debug.Log($"[Llorona] Fase {p}");
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDefeated?.Invoke();
    }

    void OnDestroy()
    {
        if (!IsDead && health != null && health.Current <= 0f) Die();
    }

    [ContextMenu("Probar: fase 2")] void TestPhase2() => SetPhase(2);
    [ContextMenu("Probar: fase 3")] void TestPhase3() => SetPhase(3);
}