using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BarrierSpell : MonoBehaviour
{
    [Header("Casting")]
    public float manaCost = 20f;
    public float spawnDistance = 2f;
    public float lifetime = 5f;

    [Header("Blocking")]
    public LayerMask projectileLayers;

    public static bool TryCast(BarrierSpell prefab, Transform view, MP mana)
    {
        if (prefab == null || view == null || mana == null)
            return false;

        Vector3 forward = Vector3.ProjectOnPlane(view.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f)
            return false;
        forward.Normalize();

        if (!mana.TrySpend(prefab.manaCost))
            return false;

        Vector3 position = view.position + forward * prefab.spawnDistance;
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
        BarrierSpell barrier = Instantiate(prefab, position, rotation);
        Destroy(barrier.gameObject, Mathf.Max(0.1f, prefab.lifetime));
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        BlockProjectile(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        BlockProjectile(other);
    }

    private void BlockProjectile(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        GameObject projectile = body != null ? body.gameObject : other.gameObject;
        int layers = (1 << other.gameObject.layer) | (1 << projectile.layer);
        if ((projectileLayers.value & layers) == 0)
            return;

        projectile.SetActive(false);
        Destroy(projectile);
    }
}
