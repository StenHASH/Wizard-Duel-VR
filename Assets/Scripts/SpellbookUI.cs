using UnityEngine;

public class SpellbookUI : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // Main Camera
    public SpellSlot[] slots;

    [Header("Gaze")]
    public float maxDistance = 5f;
    public int startingIndex = 0;

    SpellSlot hovered;
    SpellSlot selected;

    string lastHitList = null; // [ TESTING ]

    [Header("Debug")]
    public bool logGazeHits = true;

    private void Start()
    {
        Debug.Log("SpellBookUI v2 running. Camera: " +
                  (cameraTransform != null ? cameraTransform.name : "NOT ASSIGNED") +
                  ", slots: " + (slots != null ? slots.Length : 0));

        if (slots != null && slots.Length > 0)
            Select(slots[Mathf.Clamp(startingIndex, 0, slots.Length - 1)]);
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // Cast a ray from the center of the view (where the reticle is)
        SpellSlot found = null;
        float best = float.MaxValue;
        var ray = new Ray(cameraTransform.position, cameraTransform.forward);
        var hits = Physics.RaycastAll(ray, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        // Log what the gaze ray touches, only when it changes [ TESTING ]
        if (logGazeHits)
        {
            string list = "";
            foreach (var h in hits) list += h.collider.name + " ";
            if (list != lastHitList)
            {
                Debug.Log("Gaze hits: " + (list == "" ? "nothing" : list));
                lastHitList = list;
            }
        }


        foreach (var hit in hits)
        {
            var slot = hit.collider.GetComponent<SpellSlot>();
            if (slot != null && hit.distance < best)
            {
                best = hit.distance;
                found = slot;
            }
        }

        if (found != hovered)
        {
            if (hovered != null) hovered.SetHovered(false);
            hovered = found;
            if (hovered != null) hovered.SetHovered(true);
        }
    }

    // Called by PlayerController when Cast Button is pressed
    public void HandleCastButton()
    {
        if (hovered != null) Select(hovered);   // Gazing at a slot: choose it
        else CastSelected();                    // Otherwise: cast chosen spell
    }

    void Select(SpellSlot slot)
    {
        if (selected != null) selected.SetSelected(false);
        selected = slot;
        selected.SetSelected(true);
        Debug.Log("Selected spell: " + selected.spellName);
    }

    void CastSelected()
    {
        Debug.Log("Cast spell: " + (selected != null ? selected.spellName : "none selected"));
        // Later: instantiate selected.spellPrefab toward the reticle
    }
}
