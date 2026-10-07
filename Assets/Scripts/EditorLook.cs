using UnityEngine;
/// Rotates the camera with the mouse while playing in the Editor only.
/// On a device the Cardboard TrackedPoseDriver drives rotation instead.
public class EditorLook : MonoBehaviour
{
    public float sensitivity = 2f;
    float yaw, pitch;
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(1)) // right-drag to look
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
#endif
    }
}