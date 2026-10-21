using System.Text;
using UnityEngine;

public class InputWatcher : MonoBehaviour
{
    public Transform cameraTransform;   // Main Camera
    [Tooltip("Also scan JoyAxis1-28 (run Tools > Add Joystick Scan Axes first)")]
    public bool scanJoyAxes = true;
    public float peakHold = 2f;
    public Vector3 localPosition = new Vector3(0f, 0.05f, 1.5f);
    public float charSize = 0.015f;

    string[] names;
    float[] peak, peakTime, prev;
    TextMesh tm;

    void Awake()
    {
        var list = new System.Collections.Generic.List<string>
            { "Horizontal", "Vertical", "Mouse X", "Mouse Y", "Mouse ScrollWheel" };
        if (scanJoyAxes)
            for (int i = 1; i <= 28; i++) list.Add("JoyAxis" + i);

        names = list.ToArray();
        peak = new float[names.Length];
        peakTime = new float[names.Length];
        prev = new float[names.Length];

        var go = new GameObject("InputWatcherText");
        go.transform.SetParent(cameraTransform, false);
        go.transform.localPosition = localPosition;
        tm = go.AddComponent<TextMesh>();
        tm.anchor = TextAnchor.LowerCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = charSize;
        tm.fontSize = 64;
        tm.color = Color.cyan;

        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        tm.font = font;
        go.GetComponent<MeshRenderer>().material = font.material;

        Debug.Log("Joysticks: [" + string.Join(" | ", Input.GetJoystickNames()) + "]");
    }

    static float SafeRaw(string axis)
    {
        try { return Input.GetAxisRaw(axis); }
        catch { return 0f; }   // axis not defined in Input Manager
    }

    void Update()
    {
        var sb = new StringBuilder();
        sb.AppendLine("mousePresent=" + Input.mousePresent +
                      " pos=" + (int)Input.mousePosition.x + "," + (int)Input.mousePosition.y);

        for (int i = 0; i < names.Length; i++)
        {
            float v = SafeRaw(names[i]);

            if (Mathf.Abs(v) >= Mathf.Abs(peak[i]) && Mathf.Abs(v) > 0.05f)
            {
                peak[i] = v;
                peakTime[i] = Time.time;
            }
            else if (Time.time - peakTime[i] > peakHold)
            {
                peak[i] = 0f;
            }

            // One log line when an axis crosses the threshold (shows in VRLogger)
            if (Mathf.Abs(v) > 0.3f && Mathf.Abs(prev[i]) <= 0.3f)
                Debug.Log(names[i] + " = " + v.ToString("F1"));
            prev[i] = v;

            // Always list the first three; others only when active
            if (i < 3 || peak[i] != 0f)
                sb.AppendLine(names[i] + ": " + v.ToString("F2") + "  (peak " + peak[i].ToString("F2") + ")");
        }
        tm.text = sb.ToString();
    }
}