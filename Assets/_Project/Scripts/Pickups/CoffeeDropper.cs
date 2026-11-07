using UnityEngine;

[RequireComponent(typeof(Health))]
public class CoffeeDropper : MonoBehaviour
{
    [SerializeField] private CoffeeBean coffeeBeanPrefab;
    [SerializeField, Min(1)] private int minDrops = 1;
    [SerializeField, Min(1)] private int maxDrops = 3;
    [SerializeField, Min(0f)] private float scatterRadius = 0.5f;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.Died += DropCoffee;
    }

    private void OnDestroy()
    {
        if (health != null) health.Died -= DropCoffee;
    }

    private void DropCoffee()
    {
        if (coffeeBeanPrefab == null) return;

        int amount = Random.Range(minDrops, maxDrops + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * scatterRadius;
            Instantiate(coffeeBeanPrefab, transform.position + (Vector3)offset, Quaternion.identity);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Prueba: soltar café")]
    private void TestDrop() => DropCoffee();
#endif
}
