using UnityEngine;

public class BodyFollowGaze : MonoBehaviour
{
    [Tooltip("The Main Camera to follwo")]
    public Transform cameraTransform;

    [Tooltip("Higher = snappier, lower = more lag")]
    public float followSpeed = 4f;

    [Tooltip("1 = follow full head tilt, 0 = ignore it")]
    [Range(0f, 1f)] public float pitchAmount = 1f;

    [Tooltip("Ignore sideways head roll (tilting ear to shoulder")]
    public bool ignoreRoll = true;

    void LateUpdate()
    {
        Vector3 euler = cameraTransform.rotation.eulerAngles;

        // Convert pitch to -180..180 so scaling works correctly
        float pitch = Mathf.DeltaAngle(0f, euler.x) * pitchAmount;
        float roll = ignoreRoll ? 0f : euler.z;

        Quaternion target = Quaternion.Euler(pitch, euler.y, roll);

        // Frame-rate independent smoothing
        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
    }
}
