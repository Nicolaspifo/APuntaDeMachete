using System.Collections;
using UnityEngine;

/// <summary>
/// Casillas de aturdimiento del jefe. Cada vez que la Llorona empieza a cantar, aparecen cuadrados avisados en el
/// suelo FUERA del aro; al terminar el aviso, quien este encima queda aturdido. Aparecen donde el jugador suele huir
/// del aro, asi que escapar del canto tiene riesgo. Cuadrados dibujados por codigo (sin assets).
/// Sin casillas en la fase 1; despues 3 / 4 / 6.
/// </summary>
[RequireComponent(typeof(CryAttack), typeof(BossLlorona))]
public class StunTileAttack : MonoBehaviour
{
    [Tooltip("Casillas por fase (fase 1 = 0, sin aturdimiento).")]
    [SerializeField] int[] tilesPerPhase = { 0, 3, 4, 6 };
    [SerializeField, Min(0.5f)] float tileSize = 2f;
    [Tooltip("Segundos de aviso de la casilla (se queda a la vista este tiempo antes de aturdir).")]
    [SerializeField, Min(0.2f)] float warningTime = 1.2f;
    [SerializeField, Min(0.1f)] float stunDuration = 1.2f;
    [Tooltip("Margen entre el borde del aro del canto y la casilla.")]
    [SerializeField, Min(0f)] float ringMargin = 1.2f;
    [Tooltip("Dispersion de las casillas extra alrededor del jugador.")]
    [SerializeField, Min(0f)] float scatter = 4f;
    [SerializeField] Color warningColor = new Color(1f, 0.1f, 0.2f, 0.55f);

    BossLlorona boss;
    CryAttack cry;
    Transform player;
    Rigidbody2D playerBody;
    Stunnable stunnable;
    Sprite square;

    void Awake()
    {
        boss = GetComponent<BossLlorona>();
        cry = GetComponent<CryAttack>();

        var tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        square = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void Start()
    {
        FindPlayer();
        cry.CryStarted += OnCryStarted;
    }

    void OnDestroy()
    {
        if (cry != null) cry.CryStarted -= OnCryStarted;
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;

        player = p.transform;
        p.TryGetComponent(out playerBody);
        if (!p.TryGetComponent(out stunnable)) stunnable = p.AddComponent<Stunnable>();   // el estuneo no requiere montaje manual
    }

    void OnCryStarted()
    {
        if (boss.IsDead) return;
        if (player == null) FindPlayer();
        if (player == null) return;

        int count = tilesPerPhase[Mathf.Clamp(boss.Phase - 1, 0, tilesPerPhase.Length - 1)];
        for (int i = 0; i < count; i++) StartCoroutine(Tile(PickPosition(i)));
    }

    Vector2 PickPosition(int index)
    {
        Vector2 center = player.position;
        // La primera casilla cae donde estara el jugador si huye en linea recta; las demas, repartidas a su alrededor.
        if (index == 0 && playerBody != null) center += playerBody.linearVelocity * cry.WarningTime;
        else center += Random.insideUnitCircle * scatter;

        // Siempre fuera del aro: si cae dentro, se empuja hacia fuera.
        Vector2 bossPos = transform.position;
        Vector2 away = center - bossPos;
        float minDistance = cry.Radius + ringMargin + tileSize * 0.5f;
        if (away.sqrMagnitude < 0.01f) away = Random.insideUnitCircle.normalized;
        if (away.magnitude < minDistance) center = bossPos + away.normalized * minDistance;
        return center;
    }

    IEnumerator Tile(Vector2 pos)
    {
        var go = new GameObject("StunTile");
        go.transform.position = pos;
        go.transform.localScale = new Vector3(tileSize, tileSize, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.sortingOrder = 5;

        for (float t = 0f; t < warningTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(0.25f, warningColor.a, Mathf.PingPong(t * 6f, 1f));
            sr.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
            yield return null;
        }

        // Descarga: destello y aturdimiento si el jugador sigue encima.
        sr.color = Color.white;
        if (stunnable != null && player != null && !boss.IsDead)
        {
            Vector2 d = (Vector2)player.position - pos;
            float half = tileSize * 0.5f;
            if (Mathf.Abs(d.x) <= half && Mathf.Abs(d.y) <= half) stunnable.TryStun(stunDuration);
        }

        yield return new WaitForSeconds(0.15f);
        Destroy(go);
    }
}
