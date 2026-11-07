using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRuntimeStats : MonoBehaviour
{
    [SerializeField] private MacheteAttack machete;
    [SerializeField] private Health health;
    [SerializeField, Min(0f)] private float baseMoveSpeed = 5f;
    [SerializeField, Min(0f)] private float basePickupRadius = 2f;
    [SerializeField] private UpgradeData testUpgrade;
    [SerializeField, Min(0f)] private float basePoisonDuration = 3f;

    private readonly List<UpgradeData> upgrades = new();
    private readonly Dictionary<StatType, float> baseValues = new();
    private readonly Dictionary<StatType, float> current = new();

    public event Action StatsChanged;
    public IReadOnlyList<UpgradeData> Upgrades => upgrades;
    public float MoveSpeed => Get(StatType.MoveSpeed);
    public float PickupRadius => Get(StatType.PickupRadius);

    private void Awake()
    {
        if (machete == null) machete = GetComponent<MacheteAttack>();
        if (health == null) health = GetComponent<Health>();

        baseValues[StatType.Damage] = machete.Damage;
        baseValues[StatType.AttackCooldown] = machete.Cooldown;
        baseValues[StatType.AttackRange] = machete.Range;
        baseValues[StatType.MaxHealth] = health.Max;
        baseValues[StatType.MoveSpeed] = baseMoveSpeed;
        baseValues[StatType.PickupRadius] = basePickupRadius;
        baseValues[StatType.PoisonDamage] = 0f;
        baseValues[StatType.PoisonDuration] = basePoisonDuration;
        baseValues[StatType.Regeneration] = 0f;

        Recalculate();
    }

    public float Get(StatType stat) => current.TryGetValue(stat, out float value) ? value : 0f;

    public int CountOf(UpgradeData upgrade)
    {
        int count = 0;
        foreach (UpgradeData owned in upgrades)
            if (owned == upgrade) count++;
        return count;
    }

    public bool CanApply(UpgradeData upgrade) =>
        upgrade != null && CountOf(upgrade) < upgrade.MaxStacks;

    public bool Apply(UpgradeData upgrade)
    {
        if (!CanApply(upgrade)) return false;

        upgrades.Add(upgrade);
        Recalculate();
        return true;
    }

    private void Recalculate()
    {
        foreach (KeyValuePair<StatType, float> pair in baseValues)
        {
            float add = 0f;
            float multiply = 1f;

            foreach (UpgradeData upgrade in upgrades)
            foreach (StatModifier modifier in upgrade.Modifiers)
            {
                if (modifier.stat != pair.Key) continue;
                if (modifier.op == ModifierOp.Add) add += modifier.value;
                else multiply *= modifier.value;
            }

            current[pair.Key] = (pair.Value + add) * multiply;
        }

        machete.Damage = Get(StatType.Damage);
        machete.Cooldown = Get(StatType.AttackCooldown);
        machete.Range = Get(StatType.AttackRange);
        health.SetMax(Get(StatType.MaxHealth));

        StatsChanged?.Invoke();
    }

    [ContextMenu("Prueba: aplicar mejora")]
    private void TestApply()
    {
        bool applied = Apply(testUpgrade);
        Debug.Log(applied
            ? $"{testUpgrade.DisplayName}: daño {Get(StatType.Damage)}, cooldown {Get(StatType.AttackCooldown)}, vida {Get(StatType.MaxHealth)}"
            : "No se pudo aplicar: falta la mejora o llegó al máximo");
    }
}