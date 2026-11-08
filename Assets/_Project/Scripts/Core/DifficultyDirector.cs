using System;
using UnityEngine;

/// <summary>
/// Curva de dificultad por RONDAS. Una ronda es un nivel del jugador: al subir de nivel (por XP) pasa a la
/// siguiente ronda y los enemigos se endurecen; quien sube rapido llega antes al boss. Por si un jugador se
/// estanca, el tiempo tambien empuja (una ronda extra cada <see cref="secondsPerRoundFallback"/> segundos).
///
/// Cada frame evalua la curva (interpolacion lineal entre puntos) y escribe en <see cref="DifficultyScale"/> los
/// multiplicadores de vida, dano y velocidad de los enemigos, y expone el ritmo del spawner.
/// Va en un objeto vacio de la escena. Sin director el spawner usa su ritmo fijo de siempre.
/// </summary>
public class DifficultyDirector : MonoBehaviour
{
    [Serializable]
    public struct Point
    {
        [Tooltip("Ronda (= nivel del jugador) en la que se alcanzan estos valores.")]
        public float round;
        [Tooltip("Segundos entre tandas de enemigos.")]
        public float spawnInterval;
        [Tooltip("Maximo de enemigos vivos a la vez.")]
        public int maxAlive;
        public float hp;
        public float damage;
        public float speed;

        public Point(float round, float spawnInterval, int maxAlive, float hp, float damage, float speed)
        {
            this.round = round;
            this.spawnInterval = spawnInterval;
            this.maxAlive = maxAlive;
            this.hp = hp;
            this.damage = damage;
            this.speed = speed;
        }
    }

    // El jefe aparece al nivel 18 (NightTimer.bossLevel); la curva termina ahi. Despues del ultimo punto se
    // mantienen los valores. Ver docs/10 - Rondas, boss y puntaje.
    [SerializeField] private Point[] curve =
    {
        new Point(1f,  2.5f, 10, 1.0f, 1.0f, 1.00f),  // calentamiento
        new Point(6f,  1.8f, 14, 1.0f, 1.0f, 1.00f),
        new Point(11f, 1.2f, 20, 1.6f, 1.0f, 1.15f),  // presion
        new Point(16f, 0.7f, 26, 2.4f, 1.2f, 1.35f),  // escalada
        new Point(18f, 0.7f, 26, 2.4f, 1.2f, 1.35f),  // ronda del boss
    };

    [Tooltip("Respaldo por tiempo: cada tantos segundos cuenta como una ronda extra aunque no se suba de nivel.")]
    [SerializeField, Min(5f)] private float secondsPerRoundFallback = 40f;

    [Header("Depuracion")]
    [SerializeField] private bool debugLog = false;

    public static DifficultyDirector Instance { get; private set; }

    /// <summary>Segundos jugados. Usa Time.deltaTime: la pausa de las cartas no cuenta.</summary>
    public float Elapsed { get; private set; }
    /// <summary>Progreso fraccionario sobre el que se evalua la curva: max(nivel del jugador, 1 + tiempo / respaldo).</summary>
    public float Progress { get; private set; } = 1f;
    /// <summary>Ronda actual (numero entero).</summary>
    public int Round { get; private set; } = 1;
    public float SpawnInterval { get; private set; } = 3f;
    public int MaxAlive { get; private set; } = 20;

    /// <summary>Se dispara al pasar a una ronda mayor. Parametro: la nueva ronda.</summary>
    public event Action<int> RoundChanged;

    private PlayerXP playerXP;
    private float nextLog;

    private void OnEnable()
    {
        Instance = this;
        Elapsed = 0f;
        Round = 1;
        Progress = 1f;
        Evaluate(1f);
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        DifficultyScale.Hp = DifficultyScale.Damage = DifficultyScale.Speed = 1f;
    }

    private void Update()
    {
        Elapsed += Time.deltaTime;

        if (playerXP == null) playerXP = FindAnyObjectByType<PlayerXP>();
        int level = playerXP != null ? playerXP.Level : 1;

        Progress = Mathf.Max(level, 1f + Elapsed / secondsPerRoundFallback);
        Evaluate(Progress);

        int round = Mathf.FloorToInt(Progress);
        if (round > Round)
        {
            Round = round;
            RoundChanged?.Invoke(Round);
        }

        if (debugLog && Elapsed >= nextLog)
        {
            nextLog = Elapsed + 15f;
            Debug.Log($"[Director {Elapsed:0}s] ronda {Round} | intervalo {SpawnInterval:0.00} | maxVivos {MaxAlive} | vida x{DifficultyScale.Hp:0.00} | dano x{DifficultyScale.Damage:0.00} | velocidad x{DifficultyScale.Speed:0.00}", this);
        }
    }

    private void Evaluate(float progress)
    {
        if (curve == null || curve.Length == 0) return;

        // Antes del primer punto o despues del ultimo: se mantienen los valores de ese extremo.
        Point a = curve[0];
        Point b = curve[0];

        for (int i = 0; i < curve.Length; i++)
        {
            if (curve[i].round <= progress)
            {
                a = curve[i];
                b = i + 1 < curve.Length ? curve[i + 1] : curve[i];
            }
        }

        float span = b.round - a.round;
        float k = span > 0.0001f ? Mathf.Clamp01((progress - a.round) / span) : 0f;

        SpawnInterval = Mathf.Max(0.1f, Mathf.Lerp(a.spawnInterval, b.spawnInterval, k));
        MaxAlive = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(a.maxAlive, b.maxAlive, k)));
        DifficultyScale.Hp = Mathf.Lerp(a.hp, b.hp, k);
        DifficultyScale.Damage = Mathf.Lerp(a.damage, b.damage, k);
        DifficultyScale.Speed = Mathf.Lerp(a.speed, b.speed, k);
    }
}
