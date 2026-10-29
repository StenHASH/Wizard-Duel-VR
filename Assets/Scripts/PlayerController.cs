using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;   // Main Camera
    public BodyFollowGaze body;         // BodyFollowGaze on PlayerWizard
    public SpellbookUI spellBook;       // SpellbookUI on the SpellCanvas

    [Header("Buttons (VR mode @+C)")]
    public KeyCode castKey = KeyCode.JoystickButton0;       // Lower Front Button
    public KeyCode toggleKey = KeyCode.JoystickButton13;    // C

    [Tooltip("Ignore repeated preses within this time")]
    public float toggleCooldown = 0.25f;

    [Header("Movement")]
    public float moveSpeed = 2.5f;  // Keep low to minimize nausea

    [Tooltip("Controller is held sideways, so rotate the D-pad 90 degrees")]
    public bool rotateDpad = true;

    [Header("D-pad axes (found by InputWatcher)")]
    public string axisA = "JoyAxis5";   // moved when pushing "up/down"
    public string axisB = "JoyAxis6";   // moved when pushing "left/right"
    public bool swapAxes = false;
    public bool invertA = false;
    public bool invertB = false;

    [Header("D-pad pulse handling")]
    [Tooltip("Stop moving when no pulse has arrived for this many seconds")]
    public float stopDelay = 1f;

    [Tooltip("Log D-pad pulses and the measured gap (shows in VRLogger). Turn off once it works.")]
    public bool logDpad = true;

    // Body freeze toggle
    bool detached;
    float lastToggleTime = -1f;

    // D-pad state
    Vector2 heldDpad;
    float lastPulseTime = -10f;
    Vector2 prevRaw;

    private void Update()
    {
        // C pressed: press to freeze the body, press again to unfreeze
        if (Input.GetKeyDown(toggleKey) && Time.time - lastToggleTime > toggleCooldown)
        {
            detached = !detached;
            lastToggleTime = Time.time;
            Debug.Log(detached ? "Body frozen" : "Body following gaze");
        }

        body.followGaze = !detached;

        if (Input.GetKeyDown(castKey) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (spellBook != null) spellBook.HandleCastButton();
            else Debug.Log("Cast / select pressed");
        }

        Move();
    }

    // ---------- D-pad ----------

    float SafeAxis(string axisName)
    {
        try { return Input.GetAxisRaw(axisName); }
        catch { return 0f; }   // axis not defined in Input Manager
    }

    Vector2 ReadDpadRaw()
    {
        float a = SafeAxis(axisA) * (invertA ? -1f : 1f);
        float b = SafeAxis(axisB) * (invertB ? -1f : 1f);

        // Snap to -1 / 0 / 1 so it behaves like a four-way D-pad
        a = Mathf.Abs(a) > 0.5f ? Mathf.Sign(a) : 0f;
        b = Mathf.Abs(b) > 0.5f ? Mathf.Sign(b) : 0f;

        // x = up/down, y = left/right on the D-pad
        return swapAxes ? new Vector2(b, -a) : new Vector2(a, -b);
    }

    // Keep moving until no pulse has arrived for stopDelay seconds
    Vector2 ReadDpad()
    {
        Vector2 raw = ReadDpadRaw();

        if (logDpad && raw != prevRaw)
            Debug.Log("DPad raw: " + raw);
        prevRaw = raw;

        if (raw != Vector2.zero)
        {
            heldDpad = raw;            // latest direction wins
            lastPulseTime = Time.time;
        }
        else if (Time.time - lastPulseTime > stopDelay)
        {
            heldDpad = Vector2.zero;
        }
        return heldDpad;
    }

    // ---------- Movement ----------

    private Vector3 desiredMovement;
    private Rigidbody movementBody;

    void Move()
    {
        desiredMovement = Vector3.zero;
        if (cameraTransform == null) return;

        Vector2 dpad = ReadDpad();
        Vector2 input = rotateDpad ? new Vector2(dpad.y, -dpad.x) : dpad;
        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 fwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        desiredMovement = (fwd * input.y + right * input.x) * moveSpeed;
    }

    // For Movement to be blocked by barriers
    private void FixedUpdate()
    {
        if (movementBody == null)
            movementBody = GetComponent<Rigidbody>();
        if (movementBody == null || movementBody.isKinematic) return;

        Vector3 velocity = movementBody.linearVelocity;
        velocity.x = desiredMovement.x;
        velocity.z = desiredMovement.z;
        movementBody.linearVelocity = velocity;
    }
}
