using UnityEngine;
using Gizmos = Popcron.Gizmos;

/// <summary>
/// A dampened torsional oscillator using the objects transform local rotation and the rigidbody.
/// Unfortunately, the option of not requiring a rigidbody was proving more difficult than expected,
/// due to difficulty working with quaternions whilst calculating angular velocity, torque and angular
/// displacement to apply.
/// </summary>
[DisallowMultipleComponent]
//[RequireComponent(typeof(Rigidbody))]
public class TorsionalOscillator : MonoBehaviour
{
    [Tooltip("The local rotation about which oscillations are centered.")]
    public Vector3 localEquilibriumRotation = Vector3.zero;

    [Tooltip("The axes over which the oscillator applies torque. Within range [0, 1].")]
    public Vector3 torqueScale = Vector3.one;

    [Tooltip("The greater the stiffness constant, the lesser the amplitude of oscillations.")]
    public float stiffness = 100f;

    [Tooltip("The greater the damper constant, the faster that oscillations will disappear.")]
    [SerializeField] private float damper = 5f;

    [Tooltip("The center about which rotations should occur.")]
    [SerializeField] private Vector3 localPivotPosition = Vector3.zero;

    private Rigidbody _rb;

    public float angularDisplacementMagnitude;
    private Vector3 _rotAxis;

    /// <summary>
    /// Get the rigidbody component.
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = localPivotPosition;
    }

    /// <summary>
    /// Set the center of rotation.
    /// Update the rotation of the oscillator, by calculating and applying the restorative torque.
    /// </summary>
    private void FixedUpdate()
    {
        var restoringTorque = CalculateRestoringTorque();
        ApplyTorque(restoringTorque);
        _rb.centerOfMass = localPivotPosition;
    }

    /// <summary>
    /// Returns the damped restorative torque of the oscillator.
    /// The magnitude of the restorative torque is 0 at the equilibrium rotation and maximum at the amplitude of the oscillation.
    /// </summary>
    /// <returns>Damped restorative torque of the oscillator.</returns>
    private Vector3 CalculateRestoringTorque()
    {
        var deltaRotation = MathsUtils.ShortestRotation(transform.localRotation, Quaternion.Euler(localEquilibriumRotation));    
        deltaRotation.ToAngleAxis(out angularDisplacementMagnitude, out _rotAxis);
        var angularDisplacement = angularDisplacementMagnitude * Mathf.Deg2Rad * _rotAxis.normalized;
        var torque = AngularHookesLaw(angularDisplacement, _rb.angularVelocity);
        return (torque);
    }

    /// <summary>
    /// Returns the damped Hooke's torque for a given angularDisplacement and angularVelocity.
    /// </summary>
    /// <param name="angularDisplacement">The angular displacement of the oscillator from the equilibrium rotation.</param>
    /// <param name="angularVelocity">The local angular velocity of the oscillator.</param>
    /// <returns>Damped Hooke's torque</returns>
    private Vector3 AngularHookesLaw(Vector3 angularDisplacement, Vector3 angularVelocity)
    {
        var torque = (stiffness * angularDisplacement) + (damper * angularVelocity); // Damped angular Hooke's law
        torque = -torque; // Take the negative of the torque, since the torque is restorative (attractive)
        return (torque);
    }

    /// <summary>
    /// Adds a torque to the oscillator using the rigidbody.
    /// </summary>
    /// <param name="torque">The torque to be applied.</param>
    private void ApplyTorque(Vector3 torque)
    {      
        _rb.AddTorque(Vector3.Scale(torque, torqueScale));      
    }

    public bool renderGizmos = true;
    /// <summary>
    /// Draws the pivot of rotation (wire sphere), the oscillator bob (sphere) and the equilibrium (wire sphere).
    /// </summary>
    //void OnDrawGizmos()
    private void OnRenderObject()
    {
        if (renderGizmos)
        {
            var transform1 = transform;
            var bob = transform1.position;
            var axis = _rotAxis.normalized;
            var angle = angularDisplacementMagnitude;

            // Draw (wire) pivot position
            //Gizmos.color = Color.white;
            var pivotPosition = transform.TransformPoint(Vector3.Scale(localPivotPosition, MathsUtils.Invert(transform1.localScale)));
            //Gizmos.DrawWireSphere(pivotPosition, 0.7f);
            //Gizmos.Circle(pivotPosition, 0.6f, Camera.main, Color.white);
            // Draw a cross at the pivot position;
            var cross1 = new Vector3(1, 0, 1) * 0.7f;
            var cross2 = new Vector3(1, 0, -1) * 0.7f;
            Gizmos.Line(pivotPosition - cross1, pivotPosition + cross1, Color.white);
            Gizmos.Line(pivotPosition - cross2, pivotPosition + cross2, Color.white);
            //Gizmos.Line(pivotPosition - new Vector3(0, 1, 0) * 0.6f, pivotPosition + new Vector3(0, 1, 0) * 0.6f, Color.white);

            // Color goes from green (0,1,0,0) to yellow (1,1,0,0) to red (1,0,0,0).
            var color = Color.green;
            var upperAmplitude = 90f; // Approximately the upper limit of the angle amplitude within regular use
            color.r = 2f * Mathf.Clamp(angle / upperAmplitude, 0f, 0.5f);
            color.g = 2f * (1f - Mathf.Clamp(angle / upperAmplitude, 0.5f, 1f));
            //Gizmos.color = color;

            // Draw (arc) angle to equilibrium
            var equilibrium = GizmoUtils.DrawArc(pivotPosition, bob, axis, 0f, -angle / 360f, 32, color);

            // Draw (solid) bob position
            //Gizmos.DrawSphere(bob, 0.7f);
            Gizmos.Circle(bob, 0.7f, Camera.main, color);

            // Draw (wire) equilibrium position
            //Gizmos.color = Color.green;
            //Gizmos.DrawWireSphere(equilibrium, 0.7f);
            Gizmos.Circle(equilibrium, 0.7f, Camera.main, Color.green, true);
        }
    }
}
