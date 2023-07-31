using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes this collider ignore all collisions with other specified colliders.
/// </summary>
public class IgnoreColliders : MonoBehaviour
{
    [SerializeField] private List<Collider> collidersToIgnore;
    private Collider _thisCollider;

    /// <summary>
    /// Turn off collisions between this collider and other collidersToIgnore.
    /// </summary>
    private void Start()
    {
        _thisCollider = this.GetComponent<Collider>();

        foreach (var otherCollider in collidersToIgnore)
        {
            Physics.IgnoreCollision(_thisCollider, otherCollider);
        }
    }
}
