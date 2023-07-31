using UnityEngine;

/// <summary>
/// Acts to mirror a rigidbody's position and rotation.
/// This is intended to be used to avoid cases in which a rigidbody parent has a rigidbody child, avoiding strange results.
/// </summary>
public class RigidParent : MonoBehaviour
{
    [SerializeField] public Rigidbody targetRigidbody;

    /// <summary>
    /// Set the transform position and rotation to match the targetRigidbody's.
    /// </summary>
    private void Start()
    {
        if (targetRigidbody == null) return;
        var transform1 = transform;
        var transform2 = targetRigidbody.transform;
        transform1.position = transform2.position;
        transform1.rotation = transform2.rotation;
    }

    /// <summary>
    /// Update the transform position and rotation to match the targetRigidbody's.
    /// </summary>
    private void FixedUpdate()
    {
        if (targetRigidbody == null) return;
        var transform1 = transform;
        var transform2 = targetRigidbody.transform;
        transform1.position = transform2.position;
        transform1.rotation = transform2.rotation;
    }
}
