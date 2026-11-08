using System;
using TMPro;
using UnityEngine;

public class NightTimer : MonoBehaviour
{
    [Header("Tiempo")]
    [SerializeField] float duration = 180f;

    [Header("Jefe")]
    [SerializeField] GameObject bossPrefab;
    [SerializeField] Transform player;
    [SerializeField] float spawnDistance = 8f;
    [Tooltip("Opcional: si lo asignas, el jefe aparece aquí en vez de alrededor del jugador")]
    [SerializeField] Transform spawnPoint;

    [Header("Al aparecer el jefe")]
    [Tooltip("Scripts a apagar, por ejemplo el EnemySpawner")]
    [SerializeField] Behaviour[] disableWhenBossAppears;

    [Header("UI (opcional)")]
    [SerializeField] TMP_Text label;

    public float Remaining { get; private set; }
    public bool BossSpawned { get; private set; }
    public event Action<GameObject> OnBossSpawned;

    void Start()
    {
        Remaining = duration;
        UpdateLabel();
    }

    void Update()
    {
        if (BossSpawned) return;

        Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);
        UpdateLabel();

        if (Remaining <= 0f) SpawnBoss();
    }

    void SpawnBoss()
    {
        BossSpawned = true;

        foreach (var b in disableWhenBossAppears)
            if (b != null) b.enabled = false;

        Vector3 pos;
        if (spawnPoint != null)
        {
            pos = spawnPoint.position;
        }
        else
        {
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            pos = player.position + (Vector3)(dir * spawnDistance);
        }
        pos.z = 0f;

        GameObject boss = Instantiate(bossPrefab, pos, Quaternion.identity);
        boss.SetActive(true);
        OnBossSpawned?.Invoke(boss);
        Debug.Log("[Noche] ¡Apareció el jefe!");
    }

    void UpdateLabel()
    {
        if (label == null) return;
        int s = Mathf.CeilToInt(Remaining);
        label.text = BossSpawned ? "¡LA LLORONA!" : $"{s / 60}:{s % 60:00}";
    }
}