using UnityEngine;
using Gizmos = Popcron.Gizmos;

/// <summary>
/// A dampened oscillator using the objects transform local position.
/// </summary>
[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
    private Vector3 _previousDisplacement = Vector3.zero;
    private Vector3 _previousVelocity = Vector3.zero;

    [Tooltip("The local position about which oscillations are centered.")]
    public Vector3 localEquilibriumPosition = Vector3.zero;

    [Tooltip("The axes over which the oscillator applies force. Within range [0, 1].")]
    public Vector3 forceScale = Vector3.one;

    [Tooltip("The greater the stiffness constant, the lesser the amplitude of oscillations.")]
    [SerializeField] private float stiffness = 100f;

    [Tooltip("The greater the damper constant, the faster that oscillations will disappear.")]
    [SerializeField] private float damper = 2f;

    [Tooltip("The greater the mass, the lesser the amplitude of oscillations.")]
    [SerializeField] private float mass = 1f;


    /// <summary>
    /// Update the position of the oscillator, by calculating and applying the restorative force.
    /// </summary>
    private void FixedUpdate()
    {
        var restoringForce = CalculateRestoringForce();
        ApplyForce(restoringForce);
    }

    /// <summary>
    /// Returns the damped restorative force of the oscillator.
    /// The magnitude of the restorative force is 0 at the equilibrium position and maximum at the amplitude of the oscillation.
    /// </summary>
    /// <returns>Damped restorative force of the oscillator.</returns>
    private Vector3 CalculateRestoringForce()
    {
        var displacement = transform.localPosition - localEquilibriumPosition; // Displacement from the rest point. Displacement is the difference in position.
        var deltaDisplacement = displacement - _previousDisplacement;
        _previousDisplacement = displacement;
        var velocity = deltaDisplacement / Time.fixedDeltaTime; // Kinematics. Velocity is the change-in-position over time.
        var force = HookesLaw(displacement, velocity);
        return (force);
    }

    /// <summary>
    /// Returns the damped Hooke's force for a given displacement and velocity.
    /// </summary>
    /// <param name="displacement">The displacement of the oscillator from the equilibrium position.</param>
    /// <param name="velocity">The local velocity of the oscillator.</param>
    /// <returns>Damped Hooke's force</returns>
    private Vector3 HookesLaw(Vector3 displacement, Vector3 velocity)
    {
        var force = (stiffness * displacement) + (damper * velocity); // Damped Hooke's law
        force = -force; // Take the negative of the force, since the force is restorative (attractive)
        return (force);
    }

    /// <summary>
    /// Adds a force to the oscillator. Updates the transform's local position.
    /// </summary>
    /// <param name="force">The force to be applied.</param>
    public void ApplyForce(Vector3 force)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.Scale(force, forceScale)); 
        }
        else
        {
            var displacement = CalculateDisplacementDueToForce(force);
            transform.localPosition += Vector3.Scale(displacement, forceScale);
        }
    }

    /// <summary>
    /// Returns the displacement that results from applying a force over a single fixed update.
    /// </summary>
    /// <param name="force">The causative force.</param>
    /// <returns>Displacement over a single fixed update.</returns>
    private Vector3 CalculateDisplacementDueToForce(Vector3 force)
    {
        var acceleration = force / mass; // Newton's second law.
        var deltaVelocity = acceleration * Time.fixedDeltaTime; // Kinematics. Acceleration is the change in velocity over time.
        var velocity = deltaVelocity + _previousVelocity; // Calculating the updated velocity.
        _previousVelocity = velocity;
        var displacement = velocity * Time.fixedDeltaTime; // Kinematics. Velocity is the change-in-position over time.
        return (displacement);
    }

    public bool renderGizmos = true;
    /// <summary>
    /// Draws the oscillator bob (sphere) and the equilibrium (wire sphere).
    /// </summary>
    //void OnDrawGizmos()
    private void OnRenderObject()
    {
        if (renderGizmos)
        {
            var bob = transform.localPosition;
            var equilibrium = localEquilibriumPosition;
            if (transform.parent != null)
            {
                var parent = transform.parent;
                var position = parent.position;
                bob += position;
                equilibrium += position;
            }

            // Draw (wire) equilibrium position
            var color = Color.green;
            //Gizmos.color = color;
            //Gizmos.DrawWireSphere(equilibrium, 0.7f);
            Gizmos.Circle(equilibrium, 0.7f, Camera.main, color, true);

            // Draw (solid) bob position
            // Color goes from green (0,1,0,0) to yellow (1,1,0,0) to red (1,0,0,0).
            var upperAmplitude = stiffness * mass / (3f * 100f); // Approximately the upper limit of the amplitude within regular use
            color.r = 2f * Mathf.Clamp(Vector3.Magnitude(bob - equilibrium) * upperAmplitude, 0f, 0.5f);
            color.g = 2f * (1f - Mathf.Clamp(Vector3.Magnitude(bob - equilibrium) * upperAmplitude, 0.5f, 1f));
            //Gizmos.color = color;
            //Gizmos.DrawSphere(bob, 0.75f);
            Gizmos.Circle(bob, 0.7f, Camera.main, color);
            //Gizmos.DrawLine(bob, equilibrium);
            Gizmos.Line(bob, equilibrium, color);
        }
    }
}
