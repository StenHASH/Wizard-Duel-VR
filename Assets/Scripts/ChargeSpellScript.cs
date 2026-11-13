using UnityEngine;

public class ChargeSpellScript : MonoBehaviour
{
    [Header("Spell Settings")]
    public GameObject chargeBallPrefab;
    public Transform playerCamera;
    public float spawnDistance = 2f;
    public float lifetime = 5f;

    [Header("Charge Settings")]
    public float maxChargeTime = 5f;

    [Header("Lauch Settings")]
    public float minLaunchSpeed = 5f;
    public float maxLaunchSpeed = 25f;

    //private float currentChargeTime = 0f;
    private float chargeStartTime;
    public bool isCharging = false;

    public bool Charging => isCharging;
    public float ChargePercent => isCharging ? Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime) : 0f;

    Rigidbody orbRb;

    // Called when the player starts charging
    public void StartCharging()
    {
        if (isCharging) return;

        isCharging = true;
        chargeStartTime = Time.time;

        Debug.Log("Charging Spell");
    }

    // Called when the player presses the button again after/during charge
    public void ReleaseSpell()
    {
        if (!isCharging) return;

        isCharging = false;

        float held = Time.time - chargeStartTime;

        // Calculate charge percentage (0 to 1)
        float chargePercentage = Mathf.Clamp01(held / maxChargeTime);

        CastSpell(chargePercentage);

        Debug.Log("Chargespell fired");
    }

    // Spawn and launch Charge projectile
    private void CastSpell(float chargePercentage)
    {
        // Get player's gaze direction
        Vector3 forward = playerCamera.forward;
        Vector3 horizontalForward = Vector3.ProjectOnPlane(forward, Vector3.up);

        // Calculate spawn position
        Vector3 spawnPosition = playerCamera.position + forward * spawnDistance;

        // Spawn orb
        GameObject orb = Instantiate(chargeBallPrefab, spawnPosition, Quaternion.identity);
        orb.AddComponent<PlayerAttacks>();
        orbRb = orb.GetComponent<Rigidbody>();
        ChargeOrbScript orbScript = orb.GetComponent<ChargeOrbScript>();


        if (orbRb != null)
        {
            // Fixed launch angle, speed scales with charge
            float launchAngle = 45f * Mathf.Deg2Rad;
            float speed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, chargePercentage);
            Debug.Log($"speed={speed:F1}  rb velocity after set={orbRb.linearVelocity.magnitude:F1}");

            Vector3 launchDirection = horizontalForward * Mathf.Cos(launchAngle) + Vector3.up * Mathf.Sin(launchAngle);

            orbRb.linearVelocity = launchDirection * speed;
        }

        Destroy(orb, Mathf.Max(0.1f, lifetime));
    }
}
