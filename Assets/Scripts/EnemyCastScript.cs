using UnityEngine;
using System.Collections;
using Unity.Hierarchy;

public class EnemyCastScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;              // Player's Main Camera
    [SerializeField] private Transform spawnPoint;          // Orb spawn location
    [SerializeField] private GameObject chargeOrbPrefab;    // the chargeorb prefab

    [Header("Mana")]
    public MP mana;

    [Header("Cast Timing")]
    [SerializeField] private float minCastDelay = 2f;
    [SerializeField] private float maxCastDelay = 4f;

    [Header("Spell Roll")]
    [Range(0f, 1f)][SerializeField] private float chargeSpellChance = 0.33f;
    [Range(0f, 1f)][SerializeField] private float secondSpellChance = 0.66f;

    [Header("Charge Spell")]
    [SerializeField] private float chargeTime = 1f;         // telegraph so player can react
    [SerializeField] private float spawnDistance = 1.5f;    // offset in front of spawnpoint
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float launchAngleDeg = 45f;

    private bool isCasting;
    private bool manaCharge;

    [Header("Accuracy Noise (m/s added to launch speed")]
    [SerializeField] private float minNoise = 0f;
    [SerializeField] private float maxNoise = 0.5f;
    [Tooltip("If true, noise randomly adds or subtracts (short or long). If false, always adds (overshoots)")]
    [SerializeField] private bool randomSign = true;

    [Header("Barrier Spell")]
    [SerializeField] private BarrierSpell barrierPrefab;    // the Barrier prefab
    [SerializeField] private float barrierDistance = 2f;
    [SerializeField] private LayerMask playerAttackLayer;   // playerAttacks

    private GameObject activeBarrier;
    private bool BarrierActive => activeBarrier != null;    // Null check turns true once destroyed

    [Header("Player Attack Detection")]
    [SerializeField] private float minThreatSpeed = 1f;     // ignores still objects if any

    [Header("Pickable Spell")]
    [SerializeField] private LayerMask pickableLayer;       // the Pickable layer
    [SerializeField] private float pickableRange = 30f;
    [SerializeField] private Transform holdPoint;           // where the objects hover
    [SerializeField] private DamageReceiver enemyReceiver;  // the DamageReceiver
    [SerializeField] private float holdTime = 1.2f;         // hover time (as a warning to the player)
    //[SerializeField] private float aimNoiseDeg = 4f;        // random angular error on the throw

    private readonly Collider[] pickableBuffer = new Collider[64];


    private void Start()
    {
        StartCoroutine(CastLoop());
    }

    private IEnumerator CastLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minCastDelay, maxCastDelay));

            if (target == null || isCasting) continue;

            // If barrier is up, no attacking or casting
            if (BarrierActive) continue;

            float roll = Random.value;

            // < 33% then cast charge spell 
            if (roll < chargeSpellChance)
            {
                manaCharge = mana.TrySpend(25f);
                if (manaCharge) yield return StartCoroutine(CastChargeSpell());
                else Debug.Log("Enemy: Not Enough Mana");
            }
            // < 66% then cast Wall Spell
            else if (roll < secondSpellChance)
            {
                // Check for any player attacks incoming
                // If so, cast wall spell
                if (CheckPlayerAttacks())
                {
                    manaCharge = mana.TrySpend(20f);
                    if (manaCharge) CastWallSpell();
                    else Debug.Log("Enemy: Not Enough Mana");
                }

                // If no attacks, then attack with charge spell
                else
                {
                    manaCharge = mana.TrySpend(25f);
                    if (manaCharge) yield return StartCoroutine(CastChargeSpell());
                    else Debug.Log("Enemy: Not Enough Mana");
                }
            }
            // >= 66% then cast pickable spell
            else
            {
                yield return StartCoroutine(CastPickableSpell());
                
            }
        }
    }

    // --- Charge Spell Code ---
    
    // Charges and launches the orb 
    private IEnumerator CastChargeSpell()
    {
        isCasting = true;

        // First call: start charging
        Debug.Log("Enemy charging spell");
        yield return new WaitForSeconds(chargeTime);

        // Second call: launch
        if (target != null) LaunchChargeOrb();

        isCasting = false;
    }

    // Spawns and launches the charge orb at the player
    private void LaunchChargeOrb()
    {
        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position;

        // Horizontal direction and distance to the player
        Vector3 toTarget = target.position - origin;
        float height = toTarget.y;
        Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z);
        float distance = flatDir.magnitude;
        if (distance < 0.01f) return;
        flatDir /= distance;

        float angle = launchAngleDeg * Mathf.Deg2Rad;

        if (!TryGetLaunchSpeed(distance, height, angle, out float speed))
        {
            Debug.Log("Enemy: target out of reach at this angle");
            return;
        }

        // Add noise to get random orb curve
        float noise = Random.Range(minNoise, maxNoise);
        if (randomSign && Random.value < 0.5f) noise = -noise;
        speed = Mathf.Max(0.1f, speed + noise);

        Vector3 launchDir = flatDir * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle);
        Vector3 spawnPos = origin + flatDir * spawnDistance;

        GameObject orb = Instantiate(chargeOrbPrefab, spawnPos, Quaternion.identity);
        Rigidbody rb = orb.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = launchDir * speed;

        Destroy(orb, Mathf.Max(0.1f, lifetime));
        Debug.Log($"Enemy fired charge orb: speed={speed:F1} (noise {noise:F1})");
    }

    // Projectile speed needed to hit a point 'distance' away horizontally
    // and 'height' above/below the launch point, at a fixed launch angle.
    private bool TryGetLaunchSpeed(float distance, float height, float angle, out float speed)
    {
        float g = Physics.gravity.magnitude;
        float cos = Mathf.Cos(angle);
        float denom = 2f * cos * cos * (distance * Mathf.Tan(angle) - height);

        if (denom <= 0.0001f)
        {
            speed = 0f;
            return false;
        }

        speed = Mathf.Sqrt(g * distance * distance / denom);
        return true;
    }

    // --- Wall Spell Code ---

    // Checks if there are any player attacks inbound
    // returns true if yes, false if no
    private bool CheckPlayerAttacks()
    {
        for (int i = PlayerAttacks.Active.Count - 1; i >= 0; i--)
        {
            PlayerAttacks a = PlayerAttacks.Active[i];
            if (a != null && a.IsInFlight(minThreatSpeed)) return true;
        }
        return false;
    }

    private void CastWallSpell() 
    { 
        if (barrierPrefab == null || target == null) return;

        Vector3 origin = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 flatDir = Vector3.ProjectOnPlane(target.position - origin, Vector3.up);
        if (flatDir.sqrMagnitude < 0.001f) return;
        flatDir.Normalize();

        Vector3 pos = origin + flatDir * barrierDistance;
        Quaternion rot = Quaternion.LookRotation(flatDir, Vector3.up);

        BarrierSpell barrier = Instantiate(barrierPrefab, pos, rot);
        Destroy(barrier.gameObject, Mathf.Max(0.1f, barrierPrefab.lifetime));
        activeBarrier = barrier.gameObject;

        Debug.Log("Enemy cast barrier");
    }

    // Pickable Spell Code
    private IEnumerator CastPickableSpell()
    {
        Pickable p = FindPickable();

        // Nothing in range: fall back to the charge spell
        if (p == null)
        {
            Debug.Log("Enemy: No Pickables Nearby");
            manaCharge = mana.TrySpend(25f);
            if (manaCharge) yield return StartCoroutine(CastChargeSpell());
            else Debug.Log("Enemy: Not Enough Mana");
            yield break;
        }
        // Otherwise try charging mana levels
        // If successful, continue
        // Otherwise return
        else
        {
            manaCharge = mana.TrySpend(35f);
            if (!manaCharge)
            {
                Debug.Log("Enemy: Not Enough Mana");
                yield break;
            }
        }

        isCasting = true;

        Transform hold = holdPoint != null ? holdPoint : (spawnPoint != null ? spawnPoint : transform);
        p.PickUp(hold, transform, enemyReceiver);

        // Object glides to the hold point and hovers
        yield return new WaitForSeconds(holdTime);

        if (p != null && p.CurrentState == Pickable.State.Held)
        {
            Vector3 dir = target != null
                ? target.position - p.transform.position
                : transform.forward;

            p.Throw(dir);
            Debug.Log("Enemy threw a pickable");
        }

        isCasting = false;
    }

    // Nearest pickable on the ground within range, or null
    private Pickable FindPickable()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, pickableRange, pickableBuffer,
            pickableLayer, QueryTriggerInteraction.Ignore);

        Pickable best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Pickable p = pickableBuffer[i].GetComponentInParent<Pickable>();
            if (p == null || !p.isPickable) continue;        // Skips held & flying objects

            float d = (p.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }
}
