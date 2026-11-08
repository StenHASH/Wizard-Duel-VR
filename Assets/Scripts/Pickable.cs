using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Pickable : MonoBehaviour
{
    public enum State { Idle, Held, Flying, Falling }

    [Header("Damage")]
    public float damage = 5f;

    [Header("Throw")]
    public float throwSpeed = 20f;
    public float flightTime = 7f;           // Seconds of straight flight

    [Header("Pickup")]
    public float pickupSpeed = 12f;         // How fast it glides to the player's hold

    [Header("Settling")]
    public float settleSpeed = 0.3f;        // below this speed counts as "still"
    public float settleTime = 0.5f;         // must stay still this long to become pickable
    public float maxFallTime = 8f;          // in case of jitter when settling 

    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 0.95f, 0.4f);
    [Range(0f, 1f)] public float highlightStrength = 0.6f;

    public State CurrentState { get; private set; } = State.Idle;
    public bool isPickable => CurrentState == State.Idle;   // Read by BubbleCursor

    // Object variables
    private Rigidbody rb;
    private Collider[] myColliders;
    private readonly List<Collider> casterColliders = new();
    private Transform holdPoint;
    private DamageReceiver caster;
    private Vector3 homePosition;
    private Coroutine routine;

    // Highlight variables
    private Renderer[] renderers;
    private Color[] baseColors;
    private MaterialPropertyBlock block;
    private bool highlighted;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");    // URP
    private static readonly int ColorId = Shader.PropertyToID("_Color");            // Built-in

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myColliders = GetComponentsInChildren<Collider>();

        renderers = GetComponentsInChildren<Renderer>();
        baseColors = new Color[renderers.Length];
        block = new MaterialPropertyBlock();

        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].sharedMaterial;
            if (m != null && m.HasProperty(BaseColorId)) baseColors[i] = m.GetColor(BaseColorId);
            else if (m != null && m.HasProperty(ColorId)) baseColors[i] = m.GetColor(ColorId);
            else baseColors[i] = Color.white;
        }
    }

    private void Start()
    {
        homePosition = transform.position;      // after the scatterer has placed the pickables
    }

    private void Update()
    {
        if (CurrentState == State.Held && holdPoint != null)
        {
            // Frame-rate independent glide toward the hold point
            float t = 1f - Mathf.Exp(-pickupSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, holdPoint.position, t);
        }
        else if ((CurrentState == State.Flying || CurrentState == State.Falling) && transform.position.y < homePosition.y - 30f)
        {
            // Send the object back to where it started if it falls off the map
            transform.position = homePosition;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            MakePickable();
        }
    }

    // -- Highlight Pickables Section --

    public void SetHighlighted(bool on)
    {
        if (on == highlighted) return;
        highlighted = on;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (!on)
            {
                renderers[i].SetPropertyBlock(null);        // Back to material's own color
                continue;
            }

            Color c = Color.Lerp(baseColors[i], highlightColor, highlightStrength);
            renderers[i].GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            block.SetColor(ColorId, c);
            renderers[i].SetPropertyBlock(block);
        }
    }

    // -- Pick Up & Throw --
    public void PickUp(Transform hold, Transform casterRoot, DamageReceiver casterReceiver)
    {
        StopRoutine();
        holdPoint = hold;
        caster = casterReceiver;
        IgnoreCaster(casterRoot, true);
        SetHighlighted(false);

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;   // Must be zeroed before going kinematic
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;        // Cant bump the player while held
        CurrentState = State.Held;
    }

    public void Throw(Vector3 direction)
    {
        StopRoutine();
        CurrentState = State.Flying;

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;              // Straight line until it hits or time runs out
        rb.linearVelocity = direction.normalized * throwSpeed;
        rb.angularVelocity = Random.insideUnitSphere * 3f;

        routine = StartCoroutine(FlightTimer());
    }

    private IEnumerator FlightTimer()
    {
        yield return new WaitForSeconds(flightTime);
        BeginFall();                        // No hit: Gravity induced (magic run out)
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentState != State.Flying) return;       // Only the first impact counts

        DealDamage(collision.collider);
        BeginFall();                                    // Ragdoll until it settles
    }

    private void BeginFall()
    {
        StopRoutine();
        CurrentState = State.Falling;
        rb.useGravity = true;
        routine = StartCoroutine(WaitUntilSettled());
    }

    private IEnumerator WaitUntilSettled()
    {
        yield return new WaitForSeconds(0.2f);          // let it get moving first

        float still = 0f;
        float elapsed = 0f;

        // Wait for the object to settle before becoming pickable again
        // Or for maxFallTime to elapse
        while (still < settleTime && elapsed < maxFallTime)
        {
            bool slow = rb.linearVelocity.magnitude < settleSpeed && rb.angularVelocity.magnitude < settleSpeed;
            still = slow ? still + Time.deltaTime : 0f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        MakePickable();
    }

    private void MakePickable()
    {
        StopRoutine();
        IgnoreCaster(null, false);
        rb.useGravity = true;
        CurrentState = State.Idle;
    }

    // -- Damage --
    private void DealDamage(Collider other)
    {
        DamageReceiver target = other.GetComponentInParent<DamageReceiver>();
        if (target == null) target = other.transform.root.GetComponentInChildren<DamageReceiver>();
        if (target == null || target == caster) return;

        target.TakeDamage(damage);
    }

    // -- Helpers --
    private void IgnoreCaster(Transform root, bool ignore)
    {
        if (ignore)
        {
            casterColliders.Clear();
            if (root != null) casterColliders.AddRange(root.GetComponentsInChildren<Collider>());
        }

        foreach (Collider cc in casterColliders)
            foreach (Collider mc in myColliders)
                if (cc != null && mc != null) Physics.IgnoreCollision(mc, cc, ignore);

        if (!ignore) casterColliders.Clear();
    }

    private void StopRoutine()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }
}