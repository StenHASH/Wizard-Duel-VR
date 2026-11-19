using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Circular boundary around starting position")]
    [Min(0f)] public float wanderRadius = 15f;
    [Min(0f)] public float moveSpeed = 2.5f;

    [Header("Pause between moves (seconds)")]
    [Min(0f)] public float minPause = 3f;
    [Min(0f)] public float maxPause = 5f;

    [Header("Obstacles that block movement")]
    public LayerMask obstacleLayers = ~0;

    private Rigidbody body;
    private Vector3 center;
    private Vector3 destination;
    private float pauseRemaining;
    private float pauseDuration;

    private void OnValidate()
    {
        if (maxPause < minPause) maxPause = minPause;
    }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
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

        Vector3 toDestination = destination - body.position;
        float distance = Mathf.Min(moveSpeed * Time.fixedDeltaTime, toDestination.magnitude);

        // Something solid is in the way: give up on this destination
        if (distance > 0f && IsBlocked(toDestination.normalized, distance))
        {
            StartPause();
            return;
        }

        Vector3 nextPosition = Vector3.MoveTowards(
            body.position, destination, moveSpeed * Time.fixedDeltaTime);
        body.MovePosition(nextPosition);

        if ((nextPosition - destination).sqrMagnitude < 0.0001f)
        {
            pauseDuration = Random.Range(minPause, maxPause);
            if (pauseDuration > 0f) pauseRemaining = pauseDuration;
            else PickDestination();
        }
    }

    private void PickDestination()
    {
        Vector3 candidate;
        do
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            candidate = center + new Vector3(offset.x, 0f, offset.y);
        } while ((candidate - body.position).sqrMagnitude < 1f);

        destination = candidate;
    }

    private bool IsBlocked(Vector3 direction, float distance)
    {
        RaycastHit[] hits = body.SweepTestAll(
            direction, distance, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if ((obstacleLayers.value & (1 << hit.collider.gameObject.layer)) != 0)
                return true;
        }
        return false;
    }

    private void StartPause()
    {
        float pause = Random.Range(minPause, maxPause);
        if (pause > 0f) pauseRemaining = pause;
        else PickDestination();
    }
}
