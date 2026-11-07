using UnityEngine;

public class AttackRangeVisual : MonoBehaviour
{
    [Header("Ataque (asigna solo el que corresponda)")]
    [SerializeField] private MacheteAttack playerAttack;
    [SerializeField] private EnemyContactDamage enemyAttack;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField, Min(0.05f)] private float duration = 0.18f;
    [SerializeField] private bool scaleToRange = true;
    [SerializeField] private bool rotateToAttackDirection = true;

    private Color baseColor;
    private float hideAt;

    private void Awake()
    {
        if (visual == null) visual = GetComponent<SpriteRenderer>();

        if (visual != null)
            baseColor = visual.color;
        else
            Debug.LogWarning("[AttackRangeVisual] Falta asignar el SpriteRenderer.", this);

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (playerAttack != null) playerAttack.Attacked += OnPlayerAttacked;
        if (enemyAttack != null) enemyAttack.Attacked += OnEnemyAttacked;
    }

    private void OnDisable()
    {
        if (playerAttack != null) playerAttack.Attacked -= OnPlayerAttacked;
        if (enemyAttack != null) enemyAttack.Attacked -= OnEnemyAttacked;
    }

    private void OnPlayerAttacked(Vector2 direction)
    {
        // El machete golpea en un cono; giramos el visual hacia esa dirección.
        if (rotateToAttackDirection && direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        Show(playerAttack.Range);
    }

    private void OnEnemyAttacked()
    {
        Show(enemyAttack.Range);
    }

    public void Show(float range)
    {
        // El sprite debe medir ~1 unidad de diámetro para que radius = range.
        if (scaleToRange)
            transform.localScale = Vector3.one * (range * 2f);

        if (visual != null)
        {
            visual.color = baseColor;
            visual.enabled = true;
        }

        hideAt = Time.unscaledTime + duration;
    }

    private void Update()
    {
        if (visual == null || !visual.enabled) return;

        // Usamos tiempo sin escalar para que funcione incluso en pausa (timeScale = 0).
        float remaining = hideAt - Time.unscaledTime;

        if (remaining <= 0f)
        {
            SetVisible(false);
            return;
        }

        // Desvanecido suave.
        Color c = baseColor;
        c.a = baseColor.a * Mathf.Clamp01(remaining / duration);
        visual.color = c;
    }

    private void SetVisible(bool value)
    {
        if (visual != null) visual.enabled = value;
    }
}
