using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField, Min(0f)] private float damage = 5f;
    [SerializeField, Min(0f)] private float range = 1.2f;
    [SerializeField, Min(0.1f)] private float cooldown = 1f;

    private Transform target;
    private IDamageable damageable;
    private float nextHitTime;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        target = player.transform;
        player.TryGetComponent(out damageable);
    }

    private void Update()
    {
        if (target == null || damageable == null || Time.time < nextHitTime) return;

        Vector2 offset = target.position - transform.position;

        if (offset.magnitude > range) return;

        damageable.TakeDamage(damage, gameObject);
        nextHitTime = Time.time + cooldown;
    }
}