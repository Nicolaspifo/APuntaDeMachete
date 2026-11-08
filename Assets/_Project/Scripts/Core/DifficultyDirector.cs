using System;
using UnityEngine;

/// <summary>
/// Reloj de la partida y curva de dificultad. Cada frame calcula, segun el tiempo jugado, el ritmo del spawner
/// (intervalo y maximo de vivos) y los multiplicadores de vida, dano y velocidad de los enemigos, y escribe estos
/// ultimos en <see cref="DifficultyScale"/>. Entre dos puntos de la curva se interpola linealmente.
/// Va en un objeto vacio de la escena. Sin director el spawner usa su ritmo fijo de siempre.
/// </summary>
public class DifficultyDirector : MonoBehaviour
{
    [Serializable]
    public struct Point
    {
        [Tooltip("Segundo de la partida en el que se alcanzan estos valores.")]
        public float time;
        [Tooltip("Segundos entre tandas de enemigos.")]
        public float spawnInterval;
        [Tooltip("Maximo de enemigos vivos a la vez.")]
        public int maxAlive;
        public float hp;
        public float damage;
        public float speed;

        public Point(float time, float spawnInterval, int maxAlive, float hp, float damage, float speed)
        {
            this.time = time;
            this.spawnInterval = spawnInterval;
            this.maxAlive = maxAlive;
            this.hp = hp;
            this.damage = damage;
            this.speed = speed;
        }
    }

    // Partida de 6 minutos (ver docs/08 - Progresion y balance). Despues del ultimo punto se mantienen los valores.
    [SerializeField] private Point[] curve =
    {
        new Point(0f,   2.5f, 10, 1.0f, 1.0f, 1.00f),  // calentamiento
        new Point(70f,  1.8f, 14, 1.0f, 1.0f, 1.00f),  // fin del calentamiento
        new Point(180f, 1.2f, 20, 1.6f, 1.0f, 1.15f),  // presion
        new Point(315f, 0.7f, 30, 2.4f, 1.3f, 1.35f),  // escalada
        new Point(360f, 0.7f, 30, 2.4f, 1.3f, 1.35f),  // boss (se afinara en el paso 8)
    };

    [Header("Depuracion")]
    [SerializeField] private bool debugLog = false;

    public static DifficultyDirector Instance { get; private set; }

    /// <summary>Segundos jugados. Usa Time.deltaTime: la pausa de las cartas no cuenta.</summary>
    public float Elapsed { get; private set; }
    public float SpawnInterval { get; private set; } = 3f;
    public int MaxAlive { get; private set; } = 20;

    private float nextLog;

    private void OnEnable()
    {
        Instance = this;
        Evaluate(0f);
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        DifficultyScale.Hp = DifficultyScale.Damage = DifficultyScale.Speed = 1f;
    }

    private void Update()
    {
        Elapsed += Time.deltaTime;
        Evaluate(Elapsed);

        if (debugLog && Elapsed >= nextLog)
        {
            nextLog = Elapsed + 15f;
            Debug.Log($"[Director {Elapsed:0}s] intervalo {SpawnInterval:0.00} | maxVivos {MaxAlive} | vida x{DifficultyScale.Hp:0.00} | dano x{DifficultyScale.Damage:0.00} | velocidad x{DifficultyScale.Speed:0.00}", this);
        }
    }

    private void Evaluate(float t)
    {
        if (curve == null || curve.Length == 0) return;

        // Antes del primer punto o despues del ultimo: se mantienen los valores de ese extremo.
        Point a = curve[0];
        Point b = curve[0];

        for (int i = 0; i < curve.Length; i++)
        {
            if (curve[i].time <= t)
            {
                a = curve[i];
                b = i + 1 < curve.Length ? curve[i + 1] : curve[i];
            }
        }

        float span = b.time - a.time;
        float k = span > 0.0001f ? Mathf.Clamp01((t - a.time) / span) : 0f;

        SpawnInterval = Mathf.Max(0.1f, Mathf.Lerp(a.spawnInterval, b.spawnInterval, k));
        MaxAlive = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(a.maxAlive, b.maxAlive, k)));
        DifficultyScale.Hp = Mathf.Lerp(a.hp, b.hp, k);
        DifficultyScale.Damage = Mathf.Lerp(a.damage, b.damage, k);
        DifficultyScale.Speed = Mathf.Lerp(a.speed, b.speed, k);
    }
}
