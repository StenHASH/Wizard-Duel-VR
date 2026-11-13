using UnityEngine;

public class EnemyWizardAim : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("What the enemy tracks. Uses the player's head (Main Camera) for VR")]
    [SerializeField] private Transform target;

    [Header("Rotation")]
    [SerializeField] private float turnSpeed = 8f;
    [Tooltip("If true, only rotates left/right so the wizard stays upright")]
    [SerializeField] private bool yawOnly = true;

    [Header("Aim Point (for spells)")]
    [Tooltip("Where spells fire from")]
    [SerializeField] private Transform aimOrigin;


    // Other scripts (spell casting) will read this
    public Transform Target => target;
    public Vector3 DirectionToTarget { get; private set; }

    private void Update()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        DirectionToTarget = toTarget.normalized;

        if (yawOnly) toTarget.y = 0f;

        // Avoid errors when target is directly on top of npc
        if (toTarget.sqrMagnitude < 0.001f) return;

        Quaternion desired = Quaternion.LookRotation(toTarget, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}
