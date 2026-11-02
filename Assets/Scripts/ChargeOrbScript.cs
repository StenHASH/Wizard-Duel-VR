using System.Collections;
using UnityEngine;

public class ChargeOrbScript : MonoBehaviour
{
    [Header("References")]
    public Transform outerSphere;       // The translucent part of the orb
    public Renderer outerRenderer;      // Sphere's renderer

    [Header("References")]
    public Color explosionColor = new Color(1f, 0.1f, 0.1f, 0.5f);
    public float explosionScale = 1f;
    public float expandDuration = 0.3f;
    public float lingerTime = 0.5f;     // How long explosion remains before disappearing

    [Header("Damage")]
    public float damage = 30f;
    public float damageRadius = 0.5f;   // Match explosion visual
    public bool useFalloff = true;      // Full damage at the center, less at the edge
    public bool hurtsCaster = false;

    [HideInInspector] public DamageReceiver caster;

    private bool hasExploded;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return; // Other collisions after explosion are ignored
        hasExploded = true;

        DealDamage();

        // Make every collider a trigger so nothing blocks or bounces off the orb
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.isTrigger = true;

        // Freeze, then remove Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            Destroy(rb);
        }

        StartCoroutine(ExplodeVisual());
    }

    private IEnumerator ExplodeVisual()
    {
        Vector3 startScale = outerSphere.localScale;
        Vector3 endScale = Vector3.one * explosionScale;

        // .material makes an instance, so other orbs aren't affected
        Material mat = outerRenderer.material;
        Color startColor = mat.color;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, expandDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            outerSphere.localScale = Vector3.Lerp(startScale, endScale, eased);
            mat.color = Color.Lerp(startColor, explosionColor, eased);
            yield return null;
        }

        outerSphere.localScale = endScale;
        mat.color = explosionColor;

        Destroy(gameObject, lingerTime);
    }

    private void DealDamage()
    {
        // Ignore triggers so orb's own collider isnt counted
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        var alreadyHit = new System.Collections.Generic.HashSet<DamageReceiver>();

        foreach (Collider col in hits)
        {
            // Only damage each wizard once (a wizard can have several colliders)
            DamageReceiver target = col.GetComponentInParent<DamageReceiver>();
            if (target == null) target = col.transform.root.GetComponentInChildren<DamageReceiver>();
            if (target == null || !alreadyHit.Add(target)) continue;
            if (!hurtsCaster && target == caster) continue;

            float amount = damage;
            if (useFalloff)
            {
                float dist = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
                amount *= 1f - Mathf.Clamp01(dist / damageRadius) * 0.5f;   // 100% center, 50% edge
            }

            target.TakeDamage(amount);
        }
    }
}

