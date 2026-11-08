using System.Xml.Serialization;
using UnityEngine;

public class BubbleCursor : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;       // Player > Main Camera
    public Renderer bubbleRenderer;         // The translucent sphere
    public Transform ignoreRoot;            // Player root, so the ray skips the player's colliders

    [Header("Bubble")]
    public float radius = 1.5f;
    public float maxDistance = 30f;
    public Color normalColor = new Color(0.2f, 0.5f, 1f, 0.35f);
    public Color targetColor = new Color(1f, 1f, 1f, 0.45f);

    public Pickable CurrentTarget { get; private set; }

    private bool active;
    private Material bubbleMat;
    private int pickableMask;
    private readonly RaycastHit[] rayHits = new RaycastHit[16];
    private readonly Collider[] overlaps = new Collider[32];

    private void Awake()
    {
        pickableMask = LayerMask.GetMask("Pickable");
        bubbleMat = bubbleRenderer.material;    // instance
        SetActive(false);
    }

    public void SetActive(bool on)
    {
        active = on;
        bubbleRenderer.enabled = on;
    }

    private void Update()
    {
        if (!active || cameraTransform == null) return;

        Vector3 center = GetBubbleCenter();
        bubbleRenderer.transform.position = center;
        bubbleRenderer.transform.localScale = Vector3.one * radius * 2f;

        Pickable best = FindClosestPickable(center);
        if (best == CurrentTarget) return;

        if (CurrentTarget != null) CurrentTarget.SetHighlighted(false);
        CurrentTarget = best;
        if (CurrentTarget != null) CurrentTarget.SetHighlighted(true);

        bubbleMat.color = CurrentTarget != null ? targetColor : normalColor;
    }

    private Vector3 GetBubbleCenter()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        int count = Physics.RaycastNonAlloc(ray, rayHits, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        float nearest = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (ignoreRoot != null && rayHits[i].transform.IsChildOf(ignoreRoot)) continue;
            if (rayHits[i].distance < nearest) nearest = rayHits[i].distance;
        }

        // Nothing hit: park the bubble at max distance along the gaze
        if (nearest == float.MaxValue) nearest = maxDistance;
        return ray.origin + ray.direction * nearest;
    }

    private Pickable FindClosestPickable(Vector3 center)
    {
        int count = Physics.OverlapSphereNonAlloc(center, radius, overlaps, pickableMask, QueryTriggerInteraction.Ignore);


        Pickable best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Pickable p = overlaps[i].GetComponentInParent<Pickable>();
            if (p == null || !p.isPickable) continue;

            float d = (overlaps[i].ClosestPoint(center) - center).sqrMagnitude;
            if(d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }

    private void ClearTarget()
    {
        if (CurrentTarget != null) CurrentTarget.SetHighlighted(false);
        CurrentTarget = null;
        if (bubbleMat != null) bubbleMat.color = normalColor;
    }
}
