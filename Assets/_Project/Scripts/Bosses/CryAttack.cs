using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossLlorona))]
public class CryAttack : MonoBehaviour
{
    [SerializeField] float[] intervalPerPhase = { 8f, 6f, 4f };
    [SerializeField] float[] radiusPerPhase = { 4f, 5f, 6f };
    [SerializeField] float warningTime = 1f;
    [SerializeField] float stunDuration = 1.5f;
    [SerializeField] float firstCryDelay = 3f;
    [SerializeField] Color warningColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] Material lineMaterial;   // opcional

    const int Segments = 48;

    BossLlorona boss;
    Stunnable target;
    LineRenderer ring;
    float cooldown;
    bool crying;

    void Awake()
    {
        boss = GetComponent<BossLlorona>();

        ring = new GameObject("CryWarning").AddComponent<LineRenderer>();
        ring.transform.SetParent(transform, false);
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = Segments;
        ring.widthMultiplier = 0.15f;
        ring.sortingOrder = 50;
        ring.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        ring.enabled = false;
    }

    void Start()
    {
        target = FindAnyObjectByType<Stunnable>();
        if (target == null) Debug.LogWarning("CryAttack: el Player no tiene el componente Stunnable.");

        cooldown = firstCryDelay;
        boss.OnPhaseChanged += _ => cooldown = Mathf.Min(cooldown, 1f);
    }

    void Update()
    {
        if (boss.IsDead || crying) return;

        cooldown -= Time.deltaTime;
        if (cooldown <= 0f) StartCoroutine(Cry());
    }

    IEnumerator Cry()
    {
        crying = true;
        boss.MovementLocked = true;
        float radius = Pick(radiusPerPhase);

        ring.enabled = true;
        for (float t = 0f; t < warningTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(t * 6f, 1f));
            Color c = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
            ring.startColor = c;
            ring.endColor = c;
            Draw(radius);
            yield return null;
        }
        ring.enabled = false;

        if (target != null &&
            Vector2.Distance(target.transform.position, transform.position) <= radius)
            target.TryStun(stunDuration);

        boss.MovementLocked = false;
        cooldown = Pick(intervalPerPhase);
        crying = false;
    }

    void Draw(float radius)
    {
        Vector3 c = transform.position;
        for (int i = 0; i < Segments; i++)
        {
            float a = i * Mathf.PI * 2f / Segments;
            ring.SetPosition(i, c + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius);
        }
    }

    float Pick(float[] values) => values[Mathf.Clamp(boss.Phase - 1, 0, values.Length - 1)];

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusPerPhase.Length > 0 ? radiusPerPhase[0] : 4f);
    }
}