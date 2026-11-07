using UnityEngine;

public class PoisonStatus : MonoBehaviour
{
    private const float TickInterval = 0.5f;

    private IDamageable target;
    private GameObject source;
    private float damagePerSecond;
    private float endTime;
    private float nextTick;

    public static void Apply(GameObject targetObject, float dps, float duration, GameObject source)
    {
        if (targetObject == null || dps <= 0f || duration <= 0f) return;

        if (!targetObject.TryGetComponent(out PoisonStatus poison))
            poison = targetObject.AddComponent<PoisonStatus>();

        poison.Refresh(dps, duration, source);
    }

    private void Awake()
    {
        target = GetComponent<IDamageable>();
        nextTick = Time.time + TickInterval;
    }

    private void Refresh(float dps, float duration, GameObject newSource)
    {
        damagePerSecond = Mathf.Max(damagePerSecond, dps);
        endTime = Time.time + duration;
        source = newSource;
    }

    private void Update()
    {
        if (target == null || target.IsDead || Time.time >= endTime)
        {
            Destroy(this);
            return;
        }

        if (Time.time < nextTick) return;

        target.TakeDamage(damagePerSecond * TickInterval, source);
        nextTick = Time.time + TickInterval;
    }
}