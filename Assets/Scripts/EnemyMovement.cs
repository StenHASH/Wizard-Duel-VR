using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Circular boundary around starting position")]
    [Min(0f)] public float wanderRadius = 4f;
    [Min(0f)] public float moveSpeed = 2f;
    [Min(0f)] public float pauseDuration = 1f;

    private Rigidbody body;
    private Vector3 center;
    private Vector3 destination;
    private float pauseRemaining;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        center = body.position;
        PickDestination();
    }

    private void FixedUpdate()
    {
        if (pauseRemaining > 0f)
        {
            pauseRemaining -= Time.fixedDeltaTime;
            if (pauseRemaining <= 0f) PickDestination();
            return;
        }

        Vector3 nextPosition = Vector3.MoveTowards(
            body.position, destination, moveSpeed * Time.fixedDeltaTime);
        body.MovePosition(nextPosition);

        if ((nextPosition - destination).sqrMagnitude < 0.0001f)
        {
            if (pauseDuration > 0f) pauseRemaining = pauseDuration;
            else PickDestination();
        }
    }

    private void PickDestination()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        destination = center + new Vector3(offset.x, 0f, offset.y);
    }
}
