using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEditor;

public class PickableScatterer : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;                     // Relative Chance
        public Vector2 scaleRange = new Vector2(0.7f, 1.4f);
    }

    [Header("Spawn List")]
    public List<SpawnEntry> entries = new();

    [Header("Area")]
    public Vector2 areaSize = new Vector2(100f, 100f);          // Center of spawn area
    public int objectCount = 25;
    public float minSpacing = 4f;
    public float rayStartHeight = 50f;

    [Header("Keep Clear")]
    public Transform[] avoidPoints;
    public float avoidRadius = 6f;

    [Header("Ground")]
    public LayerMask groundMask = ~0;

    private readonly List<Vector3> placed = new();

    private void Start()
    {
        Scatter();
    }

    // Scatters prefab objects around the area
    public void Scatter()
    {
        if (TotalWeight() <= 0f)
        {
            Debug.LogWarning("PickableScatterer: no entries with a prefab and weight above 0");
            return;
        }

        int pickable = LayerMask.NameToLayer("Pickable");
        LayerMask mask = groundMask;
        if (pickable >= 0) mask &= ~(1 << pickable);        // rays pass through existing pickables

        int attempts = 0;
        int maxAttempts = objectCount * 20;

        while (placed.Count < objectCount && attempts++ < maxAttempts)
        {
            Vector3 origin = transform.position + new Vector3(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                rayStartHeight,
                Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f));

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                rayStartHeight * 2f, mask, QueryTriggerInteraction.Ignore))
                continue;

            if (!IsValid(hit.point)) continue;

            SpawnEntry entry = PickEntry();
            if (entry == null) break;

            Spawn(entry, hit.point);
            placed.Add(hit.point);
        }

        if (placed.Count < objectCount)
            Debug.LogWarning($"PickableScatterer placed {placed.Count}/{objectCount}. Area may be too crowded.");
    }

    // Total weight of all pickables
    private float TotalWeight()
    {
        float total = 0f;
        foreach (SpawnEntry e in entries)
            if (e != null && e.prefab != null) total += e.weight;
        return total;
    }

    // Picks a pickable prefab to spawn
    private SpawnEntry PickEntry()
    {
        float roll = Random.value * TotalWeight();

        foreach (SpawnEntry e in entries)
        {
            if (e == null || e.prefab == null || e.weight <= 0f) continue;
            if (roll < e.weight) return e;
            roll -= e.weight;
        }
        return null;
    }


    // Checks if the spawn point is valid
    private bool IsValid(Vector3 point)
    {
        foreach (Vector3 p in placed)
            if ((p - point).sqrMagnitude < minSpacing * minSpacing) return false;

        if (avoidPoints != null)
            foreach (Transform t in avoidPoints)
                if (t != null && (t.position - point).sqrMagnitude < avoidRadius * avoidRadius)
                    return false;

        return true;
    }

    private void Spawn(SpawnEntry entry, Vector3 groundPoint)
    {
        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject obj = Instantiate(entry.prefab, groundPoint, rot, transform);
        obj.transform.localScale *= Random.Range(entry.scaleRange.x, entry.scaleRange.y);

        // Sit the object on the ground using the combined bounds of all its rederers
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

            float offset = groundPoint.y - b.min.y;
            obj.transform.position += Vector3.up * (offset + 0.02f);
        }
    }
}
