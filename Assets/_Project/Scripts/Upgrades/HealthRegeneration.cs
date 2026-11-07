using UnityEngine;

[RequireComponent(typeof(Health), typeof(PlayerRuntimeStats))]
public class HealthRegeneration : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float interval = 3f;

    private Health health;
    private PlayerRuntimeStats stats;
    private float nextTick;

    private void Awake()
    {
        health = GetComponent<Health>();
        stats = GetComponent<PlayerRuntimeStats>();
    }

    private void Update()
    {
        float amount = stats.Get(StatType.Regeneration);
        if (amount <= 0f || health.IsDead || Time.time < nextTick) return;

        health.Heal(amount);
        nextTick = Time.time + interval;
    }
}