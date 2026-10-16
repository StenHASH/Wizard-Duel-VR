using System.Collections.Generic;
using UnityEngine;

public class VRLogger : MonoBehaviour
{
    public Transform cameraTransform;          // Main Camera
    public int maxLines = 12;
    public Vector3 localPosition = new Vector3(0f, -0.35f, 1.5f);
    public float charSize = 0.02f;
    public bool showWarningsAndErrors = true;

    TextMesh textMesh;
    readonly Queue<string> lines = new Queue<string>();

    void Awake()
    {
        var go = new GameObject("VRLogText");
        go.transform.SetParent(cameraTransform, false);
        go.transform.localPosition = localPosition;

        textMesh = go.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.UpperCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = charSize;
        textMesh.fontSize = 64;
        textMesh.color = Color.white;

        // Font name differs between Unity versions
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textMesh.font = font;
        go.GetComponent<MeshRenderer>().material = font.material;
    }

    void OnEnable() { Application.logMessageReceived += OnLog; }
    void OnDisable() { Application.logMessageReceived -= OnLog; }

    void OnLog(string msg, string stack, LogType type)
    {
        if (type != LogType.Log && !showWarningsAndErrors) return;

        string prefix = type == LogType.Error || type == LogType.Exception ? "[E] "
                      : type == LogType.Warning ? "[W] " : "";
        lines.Enqueue(prefix + msg);
        while (lines.Count > maxLines) lines.Dequeue();
        textMesh.text = string.Join("\n", lines);
    }
}