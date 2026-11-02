using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    public HP health;   // Associated wizard's HP component

    public void TakeDamage(float amount)
    {
        if (health != null) health.TakeDamage(amount);
    }
}
