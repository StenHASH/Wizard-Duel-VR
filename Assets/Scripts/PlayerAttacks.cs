using UnityEngine;
using System.Collections.Generic;

public class PlayerAttacks : MonoBehaviour
{
    public static readonly List<PlayerAttacks> Active = new List<PlayerAttacks>();

    private Rigidbody rb;

    private void Awake() => rb = GetComponent<Rigidbody>();
    private void OnEnable() => Active.Add(this);
    private void OnDisable() => Active.Remove(this);

    public bool IsInFlight(float minSpeed) => rb != null && rb.linearVelocity.sqrMagnitude > minSpeed * minSpeed;
}
