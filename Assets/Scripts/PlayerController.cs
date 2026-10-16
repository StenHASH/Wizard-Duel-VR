using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public BodyFollowGaze body;

    [Header("Buttons (VR mode @+C)")]
    public KeyCode castKey = KeyCode.JoystickButton0; // Lower Front Button
    public KeyCode toggleKey = KeyCode.JoystickButton13; // C

    [Tooltip("Ignore repeated preses within this time")]
    public float toggleCooldown = 0.25f;

    [Header("Movement")]
    public float moveSpeed = 2.5f;

    bool detached;
    float lastToggleTime = -1f;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && Time.time - lastToggleTime > toggleCooldown)
        {
            detached = !detached;
            lastToggleTime = Time.time;
            Debug.Log(detached ? "Body frozen" : "Body following gaze");
        }

        body.followGaze = !detached;

        if (Input.GetKeyDown(castKey))
            Debug.Log("Cast Pressed");

        Move();
    }

    void Move()
    {
        Vector2 stick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (stick.sqrMagnitude < 0.04f) return;

        // Walk relative to where you're looking, flattened to the ground
        Vector3 fwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 move = (fwd * stick.y + right * stick.x) * moveSpeed;

        transform.position += move * Time.deltaTime;
    }
}
