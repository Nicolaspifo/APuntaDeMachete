using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 4f;
    [SerializeField, Min(0f)] private float stopDistance = 1.3f;

    private Rigidbody2D body;
    private Transform target;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector2 direction = (Vector2)target.position - body.position;

        if (direction.magnitude <= stopDistance) return;

        Vector2 step = direction.normalized * (speed * Time.fixedDeltaTime);
        body.MovePosition(body.position + step);
    }
}