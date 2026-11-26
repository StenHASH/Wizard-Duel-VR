using UnityEngine;

public class PlayerDeathScript : MonoBehaviour
{
    [Header("Player Controls")]
    [SerializeField] private HP health;
    [SerializeField] private PlayerController controller;
    [SerializeField] private PickableSpell pickableSpell;
    [SerializeField] private ChargeSpellScript chargeSpell;

    [Header("Enemy Controls")]
    [SerializeField] private EnemyCastScript cast;
    [SerializeField] private EnemyWizardAim aim;
    [SerializeField] private EnemyMovement movement;

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
        if (chargeSpell != null) chargeSpell.CancelCharge();
        if (pickableSpell != null) pickableSpell.Drop();
        if (controller != null) controller.Stop();
        Debug.Log("Player wizard stopped");

        if (cast != null) cast.Stop();
        if (aim != null) aim.Stop();
        if (movement != null) movement.Stop();
        Debug.Log("Enemy wizard stopped");
    }
}
