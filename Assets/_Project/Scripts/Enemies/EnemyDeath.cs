using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    private Health health;

    private void Awake() => health = GetComponent<Health>();
    private void OnEnable() => health.Died += HandleDeath;
    private void OnDisable() => health.Died -= HandleDeath;

    private void HandleDeath() => Destroy(gameObject);
}