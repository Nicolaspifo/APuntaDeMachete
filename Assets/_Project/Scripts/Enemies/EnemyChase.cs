using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 4f;
    [SerializeField, Min(0f)] private float stopDistance = 1.3f;

    private Rigidbody2D body;
    private Transform target;
    private EnemyContactDamage contact;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        contact = GetComponent<EnemyContactDamage>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        float stop = contact !=null ? contact.Range *0.9f : stopDistance;
        Vector2 direction = (Vector2)target.position - body.position;

        if (direction.sqrMagnitude <= stop * stop) return;
        body.MovePosition(body.position + direction.normalized * (speed * Time.fixedDeltaTime));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}