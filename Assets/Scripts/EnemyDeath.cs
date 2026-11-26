using UnityEngine;
using UnityEngine.Rendering;

public class EnemyDeath : MonoBehaviour
{
    [Header("Enemy Controls")]
    [SerializeField] private HP health;
    [SerializeField] private EnemyCastScript cast;
    [SerializeField] private EnemyWizardAim aim;
    [SerializeField] private EnemyMovement movement;

    [Header("Player Controls")]
    [SerializeField] private PlayerController controller;
    [SerializeField] private PickableSpell pickableSpell;
    [SerializeField] private ChargeSpellScript chargeSpell;

    private void OnEnable()
    {
        if (health != null) health.Died += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null) health.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (cast != null) cast.Stop();
        if (aim != null) aim.Stop();
        if (movement != null) movement.Stop();
        Debug.Log("Enemy wizard stopped");

        if (chargeSpell != null) chargeSpell.CancelCharge();
        if (pickableSpell != null) pickableSpell.Drop();
        if (controller != null) controller.Stop();
        Debug.Log("Player wizard stopped");
    }
}
