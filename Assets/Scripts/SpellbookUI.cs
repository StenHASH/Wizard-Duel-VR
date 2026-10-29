using UnityEngine;

public class SpellbookUI : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // Main Camera
    public SpellSlot[] slots;
    public BarrierSpell barrierPrefab;

    [Header("Gaze")]
    public float maxDistance = 5f;
    public int startingIndex = 0;

    SpellSlot hovered;
    SpellSlot selected;

    [Header("Mana")]
    public MP mana;

    //string lastHitList = null; // [ TESTING ]

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
        /*
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
        */


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
        if (!selected)
        {
            Debug.Log("No Spell Selected");
            return;
        }
        else if (selected.spellName == "ChargeSpell")
        {
            // Cast Charge Spell
            bool test = mana.TrySpend(25f);
            if (test) Debug.Log("ChargeSpell Cast, Mana deducted: 25");
            else Debug.Log("Not Enough Mana");
        }
        else if (selected.spellName == "WallSpell")
        {
            // Cast Wall Spell
            bool test = BarrierSpell.TryCast(barrierPrefab, cameraTransform, mana);
            if (test) Debug.Log("WallSpell cast");
            else Debug.Log("WallSpell failed: check mana, prefab, and camera references");
        }
        else if (selected.spellName == "FireballSpell")
        {
            // Cast Fireball Spell
            bool test = mana.TrySpend(40f);
            if (test) Debug.Log("Fireball Cast, Mana deducted: 40");
            else Debug.Log("Not Enough Mana");
        }   
    }
}
