using UnityEngine;

/// <summary>
/// Medidor de partida para balancear: imprime una linea de resumen cada N segundos.
/// Solo escucha eventos y cuenta; no modifica nada del juego.
/// </summary>
public class RunMetrics : MonoBehaviour
{
    [SerializeField] private PlayerXP playerXP;
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerRuntimeStats stats;
    [SerializeField, Min(5f)] private float logInterval = 30f;

    private float elapsed;      // tiempo de partida; usa Time.deltaTime, asi la pausa de las cartas no cuenta
    private float nextLog;
    private int kills;
    private float minHealth;
    private float damageTaken;
    private float lastHealth;
    private float firstLevelTime = -1f;

    private void OnEnable()
    {
        GameEvents.EnemyKilled += OnEnemyKilled;
        if (playerXP != null) playerXP.OnLevelUp += OnLevelUp;
        if (playerHealth != null) playerHealth.Changed += OnHealthChanged;
    }

    private void OnDisable()
    {
        // Siempre desuscribirse: GameEvents es estatico y sobrevive a este objeto.
        GameEvents.EnemyKilled -= OnEnemyKilled;
        if (playerXP != null) playerXP.OnLevelUp -= OnLevelUp;
        if (playerHealth != null) playerHealth.Changed -= OnHealthChanged;
    }

    private void Start()
    {
        nextLog = logInterval;
        if (playerHealth != null) lastHealth = minHealth = playerHealth.Current;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed < nextLog) return;

        nextLog += logInterval;
        LogSummary();
    }

    private void OnEnemyKilled(GameObject enemy) => kills++;

    private void OnLevelUp(int level)
    {
        if (firstLevelTime < 0f) firstLevelTime = elapsed;
    }

    private void OnHealthChanged(float current, float max)
    {
        if (current < lastHealth) damageTaken += lastHealth - current; // solo cuenta bajadas
        lastHealth = current;
        minHealth = Mathf.Min(minHealth, current);
    }

    private void LogSummary()
    {
        int alive = FindObjectsByType<EnemyContactDamage>(FindObjectsSortMode.None).Length;
        int minutes = (int)(elapsed / 60f);
        int seconds = (int)(elapsed % 60f);

        string level = playerXP != null ? playerXP.Level.ToString() : "?";
        string hp = playerHealth != null ? $"{playerHealth.Current:0}/{playerHealth.Max:0} (min {minHealth:0})" : "?";
        string speed = stats != null ? $"x{stats.MoveSpeedMultiplier:0.00}" : "?";
        string first = firstLevelTime >= 0f ? $"{firstLevelTime:0}s" : "-";

        string difficulty = $"vida x{DifficultyScale.Hp:0.0} dmg x{DifficultyScale.Damage:0.0} vel x{DifficultyScale.Speed:0.00}";

        Debug.Log($"[Run {minutes:00}:{seconds:00}] nivel {level} (1er nivel a {first}) | kills {kills} | vivos {alive} | vida {hp} | daño recibido {damageTaken:0} | velocidad {speed} | enemigos: {difficulty}", this);

        // Acierto por tipo: ataques que empezaron vs golpes que realmente conectaron.
        foreach (var pair in EnemyContactDamage.WindupCount)
        {
            EnemyContactDamage.HitCount.TryGetValue(pair.Key, out int hits);
            Debug.Log($"[Run]   {pair.Key}: {hits} golpes acertados de {pair.Value} ataques ({100f * hits / pair.Value:0}%)", this);
        }

        minHealth = playerHealth != null ? playerHealth.Current : 0f; // el minimo se mide por intervalo
    }
}
