using UnityEngine;

public class PickableSpell : MonoBehaviour
{
    public BubbleCursor bubbleCursor;
    public Transform cameraTransform;       // Player > Main Camera
    public Transform holdPoint;             // Empty child of the camera
    public Transform casterRoot;            // Player root, so pickables ignore your colliders
    public DamageReceiver casterReceiver;   // PlayerWizards DamageReceiver

    private Pickable held;

    public bool IsHolding => held != null;

    // Called by SpellbookUI when the cast button is pressed with this spell selected
    public void Cast()
    {
        if (held == null)
        {
            Pickable target = bubbleCursor.CurrentTarget;
            if (target == null) return;

            held = target;
            held.PickUp(holdPoint, casterRoot, casterReceiver);
            bubbleCursor.SetActive(false);          // Hide the bubble so it doesnt block the view
        }
        else
        {
            held.Throw(cameraTransform.forward);
            held = null;
            bubbleCursor.SetActive(true);
        }
    }
}

