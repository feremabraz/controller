using UnityEngine;

/// <summary>
/// Makes a material feel extra bouncy, for when a bounce co-efficient > 1 is desired.
/// </summary>
public class ExtraBouncy : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private float extraBounceMultiplier = 10f;
    [SerializeField] private bool shouldBounceBack = true;

    /// <summary>
    /// Define the rigid body.
    /// </summary>
    private void Start()
    {
        _rb = this.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Apply the ExtraBounce method for all collisions of this rigid body / collider with another.
    /// </summary>
    /// <param name="collision">The other rigid body / collider involved in this collision.</param>
    private void OnCollisionEnter(Collision collision)
    {
        ExtraBounce(collision);
    }

    /// <summary>
    /// Applies a force to this rigid body such as to mimic a greater bounciness.
    /// Additionally applies the force to any attached oscillator, such as for squash and stretch.
    /// </summary>
    [SerializeField] private Oscillator optionalOscillator;
    private void ExtraBounce(Collision collision)
    {
        var impulse = collision.impulse;

        var minImp = Mathf.Log(2f);
        var imp = Mathf.Log(impulse.magnitude);

        imp = Mathf.Clamp(imp, minImp, Mathf.Infinity);
        var force = collision.GetContact(0).normal * imp / Time.fixedDeltaTime;

        var extraBounceForce = force * extraBounceMultiplier; // * collision.gameObject.GetComponent<Collider>().material.bounciness;

        _rb.AddForceAtPosition(extraBounceForce, collision.GetContact(0).point);
        if (shouldBounceBack)
        {
            try
            {
                collision.rigidbody.AddForce(-extraBounceForce);
            }
            catch
            {
                // Ignored.
            }
        }

        if (optionalOscillator == null) return;

        // Squash and Stretch.
        var oscillatorForce = optionalOscillator.transform.InverseTransformDirection(extraBounceForce);
        for (var i = 0; i < 3; i++)
        {
            // Make the extraBounceForce applied to the oscillator in the negative direction (should compress first).
            if (oscillatorForce[i] < 0)
            {
                oscillatorForce[i] *= -1;
            }
        }

        optionalOscillator.ApplyForce(oscillatorForce);
    }
}
