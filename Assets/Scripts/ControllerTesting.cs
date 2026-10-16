using System;
using UnityEngine;
public class ControllerTesting : MonoBehaviour
{
    void Update()
    {
        foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
            if (Input.GetKeyDown(k))
                Debug.Log("Pressed: " + k);

        float h = Input.GetAxis("Horizontal"), v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) > 0.2f || Mathf.Abs(v) > 0.2f)
            Debug.Log($"Stick: {h:F2}, {v:F2}");
    }
}
