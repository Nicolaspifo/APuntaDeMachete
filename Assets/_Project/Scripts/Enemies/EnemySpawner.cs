using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Vector2 arenaHalfSize = new Vector2(10f, 10f);
    [SerializeField] float spawnInterval = 3f;
    [SerializeField] float minInterval = 0.5f;
    [SerializeField] float intervalDecreasePerSecond = 0.02f;

    float timer;
    float elapsed;

    void Update()
    {
        timer += Time.deltaTime;
        elapsed += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }

        if (elapsed >= 1f)
        {
            spawnInterval = Mathf.Max(minInterval, spawnInterval - intervalDecreaseSecond);
            elapsed = 0f;
        }
    }

    Vector3 RandomBorderPosition()
    {
        float x = Random.Range(-arenaHalfSize.x, arenaHalfSize.x);
        float y = Random.Range(-arenaHalfSize.y, arenaHalfSize.y);
        Vector3 center = transform.position;

        switch (Random.Range(0, 4))
        {
            case 0: return new Vector3(center.x - arenaHalfSize.x, center.y + y, center.z); // Left
            case 1: return new Vector3(center.x + arenaHalfSize.x, center.y + y, center.z); // Right
            case 2: return new Vector3(center.x + x, center.y - arenaHalfSize.y, center.z); // Bottom
            default: return new Vector3(center.x + x, center.y + arenaHalfSize.y, center.z); // Top
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(arenaHalfSize.x * 2, arenaHalfSize.y * 2, 0f));
    }
}
