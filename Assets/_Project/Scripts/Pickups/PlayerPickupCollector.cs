using UnityEngine;

public class PlayerPickupCollector : MonoBehaviour
{
    public static PlayerPickupCollector Instance { get; private set; }

    [Header("Radio de recolección")]
    [SerializeField, Min(0f)] private float pickupRadius = 2.5f;
    [SerializeField] private LayerMask pickupLayers = ~0;

    private ContactFilter2D filter;
    private readonly Collider2D[] hits = new Collider2D[32];

    public float PickupRadius => pickupRadius;
    public int CoffeeCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = pickupLayers
        };
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        int count = Physics2D.OverlapCircle(
            transform.position, pickupRadius, filter, hits);

        for (int i = 0; i < count; i++)
        {
            CoffeeBean bean = hits[i].GetComponent<CoffeeBean>();
            if (bean != null)
                bean.AttractTo(transform, Time.deltaTime);
        }
    }

    public void AddCoffee(int amount)
    {
        if (amount <= 0) return;

        CoffeeCount += amount;
        GameEvents.RaiseCoffeeChanged(CoffeeCount);
        Debug.Log($"[Café] Total: {CoffeeCount}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.75f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}