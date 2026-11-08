using System;
using TMPro;
using UnityEngine;

public class NightTimer : MonoBehaviour
{
    [Header("Tiempo (respaldo)")]
    [Tooltip("Si el jugador no llega al nivel del jefe, este tiempo largo lo hace aparecer igualmente.")]
    [SerializeField] float duration = 180f;

    [Header("Aparicion por nivel (rondas)")]
    [Tooltip("Nivel del jugador al que aparece el jefe: cada nivel es una ronda, asi que quien sube mas rapido llega antes.")]
    [SerializeField, Min(2)] int bossLevel = 18;

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

    public int BossLevel => bossLevel;

    PlayerXP playerXP;

    void Start()
    {
        Remaining = duration;

        if (player != null && player.TryGetComponent(out playerXP))
            playerXP.OnLevelUp += OnLevelUp;
        else
            Debug.LogWarning("NightTimer: no se encontro PlayerXP en el jugador; el jefe solo saldra por tiempo.", this);

        UpdateLabel();
    }

    void OnDestroy()
    {
        if (playerXP != null) playerXP.OnLevelUp -= OnLevelUp;
    }

    // Al llegar al nivel del jefe se completa la ultima ronda y aparece.
    void OnLevelUp(int level)
    {
        if (!BossSpawned && level >= bossLevel) SpawnBoss();
    }

    void Update()
    {
        if (BossSpawned) return;

        Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);
        UpdateLabel();

        if (Remaining <= 0f) SpawnBoss();
    }

    [ContextMenu("Probar: aparecer jefe ahora")]
    void SpawnBoss()
    {
        if (BossSpawned) return;
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

        // Con rondas por XP el HUD muestra el avance hacia el jefe (nivel actual / nivel del jefe), no una cuenta atras.
        int level = playerXP != null ? playerXP.Level : 1;
        label.text = BossSpawned ? "¡LA LLORONA!" : $"RONDA {level}/{bossLevel}";
    }
}