using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpellSlot : MonoBehaviour
{
    [Header("Spell")]
    public string spellName = "Spell";

    [Tooltip("Child Image that will show the spell picture later")]
    public Image icon;

    // Later: public GameObject spellPrefab;  (the spell object / script to cast)

    [Header("Look")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.75f, 0.9f, 1f, 1f);
    public Color selectedColor = new Color(1f, 0.9f, 0.4f, 1f);
    public float hoverScale = 1.1f;

    Image bg;
    Vector3 baseScale;
    bool hovered, selected;

    private void Awake()
    {
        bg = GetComponent<Image>();
        baseScale = transform.localScale;

        // Add 3D collider for the slot
        var rt = (RectTransform)transform;

        var box = GetComponent<BoxCollider>();
        if (box == null) box = gameObject.AddComponent<BoxCollider>();

        box.size = new Vector3(rt.rect.width, rt.rect.height, 0.01f);
        box.center = Vector3.zero;
        
        Refresh();
    }

    void Refresh()
    {
        bg.color = selected ? selectedColor : hovered ? hoverColor : normalColor;
        transform.localScale = hovered ? baseScale * hoverScale : baseScale;
    }

    public void SetHovered(bool status)
    {
        hovered = status;
        Refresh();
    }

    public void SetSelected(bool status)
    {
        selected = status;
        Refresh();
    }
}
