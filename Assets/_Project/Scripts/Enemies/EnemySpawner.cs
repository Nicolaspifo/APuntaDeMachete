using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;
    }

    [Header("Enemigos")]
    [SerializeField] private EnemyEntry[] enemies;
    [SerializeField, Min(1)] private int enemiesPerSpawn = 1;
    [SerializeField, Min(1)] private int maxAlive = 20;

    [Header("Zona de spawn")]
    [SerializeField] private Vector2 arenaHalfSize = new Vector2(10f, 10f);

    [Header("Ritmo")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 3f;
    [SerializeField, Min(0.1f)] private float minInterval = 0.5f;
    [SerializeField, Min(0f)] private float intervalDecreasePerSecond = 0.02f;

    private readonly List<GameObject> alive = new();
    private float timer;
    private float elapsed;
    private float currentInterval;

    private void Awake()
    {
        currentInterval = spawnInterval;

        if (enemies == null || enemies.Length == 0)
            Debug.LogWarning("[EnemySpawner] No hay enemigos configurados.", this);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        elapsed += Time.deltaTime;

        if (timer >= currentInterval)
        {
            SpawnEnemies();
            timer = 0f;
        }

        if (elapsed >= 1f)
        {
            currentInterval = Mathf.Max(
                minInterval,
                currentInterval - intervalDecreasePerSecond);

            elapsed = 0f;
        }
    }

    private void SpawnEnemies()
    {
        // Limpia enemigos que ya fueron destruidos.
        alive.RemoveAll(enemy => enemy == null);

        for (int i = 0; i < enemiesPerSpawn; i++)
        {
            if (alive.Count >= maxAlive) return;

            GameObject prefab = PickRandomPrefab();
            if (prefab == null) continue;

            GameObject enemy = Instantiate(
                prefab,
                RandomBorderPosition(),
                Quaternion.identity);

            alive.Add(enemy);
        }
    }

    private GameObject PickRandomPrefab()
    {
        float totalWeight = 0f;

        foreach (EnemyEntry entry in enemies)
        {
            if (entry == null || entry.prefab == null || entry.weight <= 0f)
                continue;

            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f) return null;

        float roll = Random.Range(0f, totalWeight);

        foreach (EnemyEntry entry in enemies)
        {
            if (entry == null || entry.prefab == null || entry.weight <= 0f)
                continue;

            roll -= entry.weight;

            if (roll <= 0f)
                return entry.prefab;
        }

        return null;
    }

    private Vector3 RandomBorderPosition()
    {
        float x = Random.Range(-arenaHalfSize.x, arenaHalfSize.x);
        float y = Random.Range(-arenaHalfSize.y, arenaHalfSize.y);
        Vector3 center = transform.position;

        switch (Random.Range(0, 4))
        {
            case 0:
                return new Vector3(center.x - arenaHalfSize.x, center.y + y, center.z);

            case 1:
                return new Vector3(center.x + arenaHalfSize.x, center.y + y, center.z);

            case 2:
                return new Vector3(center.x + x, center.y - arenaHalfSize.y, center.z);

            default:
                return new Vector3(center.x + x, center.y + arenaHalfSize.y, center.z);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Prueba: spawnear ahora")]
    private void TestSpawn()
    {
        if (Application.isPlaying)
            SpawnEnemies();
    }
#endif

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(arenaHalfSize.x * 2f, arenaHalfSize.y * 2f, 0f));
    }
}