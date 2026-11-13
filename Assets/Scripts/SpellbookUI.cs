using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellbookUI : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // Main Camera
    public SpellSlot[] slots;
    public BarrierSpell barrierPrefab;
    public ChargeSpellScript chargeSpell;

    [Header("Gaze")]
    public float maxDistance = 5f;
    public int startingIndex = 0;

    SpellSlot hovered;
    SpellSlot selected;

    [Header("Mana")]
    public MP mana;

    [Header("ChargeBar")]
    public GameObject chargeBarRoot;    // ChargeBar object
    public Image chargeBarFill;    // ChargeBarFill image
    public TMP_Text chargeBarText;

    [Header("PickableSpell")]
    public BubbleCursor bubbleCursor;
    public PickableSpell pickableSpell;

    //string lastHitList = null; // [ TESTING ]

    [Header("Debug")]
    public bool logGazeHits = true;

    // ChargeSpell
    private bool isChargingSpell = false;

    private void Start()
    {
        Debug.Log("SpellBookUI v2 running. Camera: " +
                  (cameraTransform != null ? cameraTransform.name : "NOT ASSIGNED") +
                  ", slots: " + (slots != null ? slots.Length : 0));

        if (slots != null && slots.Length > 0)
            Select(slots[Mathf.Clamp(startingIndex, 0, slots.Length - 1)]);

        // Hide chargebar initially
        if (chargeBarRoot != null) chargeBarRoot.SetActive(false);
    }

    void Update()
    {
        if (isChargingSpell && chargeBarFill != null)
        {
            float percent = chargeSpell.ChargePercent;
            if (chargeBarFill != null) chargeBarFill.fillAmount = percent;
            if (chargeBarText != null) chargeBarText.text = Mathf.RoundToInt(percent * 100f) + "%";
        }
            

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
        // Prevent switching spells if holding a pickable object with the pickable spell
        bool holding = pickableSpell != null && pickableSpell.IsHolding;

        if (hovered != null && !holding) Select(hovered);   // Gazing at a slot: choose it
        else CastSelected();                    // Otherwise: cast chosen spell
    }

    void Select(SpellSlot slot)
    {
        if (selected != null) selected.SetSelected(false);
        selected = slot;
        selected.SetSelected(true);

        if (bubbleCursor != null) bubbleCursor.SetActive(selected.spellName == "PickableSpell");

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

            // Checks if charge spell has been charging or not
            // If not, try charging spell
            if (!isChargingSpell)
            {
                // Charge mana, then start charging spell
                bool trySpend = mana.TrySpend(25f);
                if (trySpend)
                {
                    // Enough mana, charge spell
                    chargeSpell.StartCharging();
                    isChargingSpell = true;

                    if (chargeBarRoot != null) chargeBarRoot.SetActive(true);
                    if (chargeBarFill != null) chargeBarFill.fillAmount = 0f;
                    if (chargeBarText != null) chargeBarText.text = "0%";
                }
                else
                {
                    // Not enough Mana, cancel charge
                    Debug.Log("Not Enough Mana");
                    return;
                }
            }
            // Spell charged, cast spell
            else
            {
                chargeSpell.ReleaseSpell();
                isChargingSpell = false;

                if (chargeBarRoot != null) chargeBarRoot.SetActive(false);
            }
        }
        else if (selected.spellName == "WallSpell" || Input.GetKeyDown(KeyCode.Space))
        {
            // Cast Wall Spell
            bool test = BarrierSpell.TryCast(barrierPrefab, cameraTransform, mana);
            if (test) Debug.Log("WallSpell cast");
        }
        else if (selected.spellName == "PickableSpell")
        {
            // Cast Pickable Spell
            bool test = mana.TrySpend(35f);
            if (test)
            {
                if (pickableSpell != null) pickableSpell.Cast();
                Debug.Log("PickableSpell Cast");
            }
            else Debug.Log("Not Enough Mana");
        }
    }
}
