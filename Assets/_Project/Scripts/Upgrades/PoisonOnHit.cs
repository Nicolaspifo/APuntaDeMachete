using UnityEngine;

[RequireComponent(typeof(MacheteAttack), typeof(PlayerRuntimeStats))]
public class PoisonOnHit : MonoBehaviour
{
    private MacheteAttack machete;
    private PlayerRuntimeStats stats;

    private void Awake()
    {
        machete = GetComponent<MacheteAttack>();
        stats = GetComponent<PlayerRuntimeStats>();
    }

    private void OnEnable() => machete.Hit += HandleHit;
    private void OnDisable() => machete.Hit -= HandleHit;

    private void HandleHit(GameObject target)
    {
        float dps = stats.Get(StatType.PoisonDamage);
        if (dps <= 0f) return;

        PoisonStatus.Apply(target, dps, stats.Get(StatType.PoisonDuration), gameObject);
    }
}