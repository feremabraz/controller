using UnityEngine;
using UnityEngine.InputSystem;
using Gizmos = Popcron.Gizmos;

/// <summary>
/// Various input-based toggles.
/// </summary>
[ExecuteAlways]
public class ToggleGizmos : MonoBehaviour
{
    [SerializeField] private Oscillator playerOscillator;

    /// <summary>
    /// Gathers and initially disables all gizmos.
    /// </summary>
    private void Start()
    {
        // Ensure Gizmos are enabled;
        Gizmos.Enabled = true;

        // Begin with all gizmos off
        //Gizmos.Enabled = !Gizmos.Enabled;
        var oscillators = GameObject.FindObjectsOfType<Oscillator>();
        var torsionalOscillators = GameObject.FindObjectsOfType<TorsionalOscillator>();

        foreach (var oscillator in oscillators)
        {
            oscillator.renderGizmos = false;
        }
        foreach (var torsionalOscillator in torsionalOscillators)
        {
            torsionalOscillator.renderGizmos = false;
        }
    }

    /// <summary>
    /// Toggles the gizmos belonging to all Torsional Oscillators.
    /// </summary>
    /// <param name="context">The input's context.</param>
    public void TorsionalOscillatorGizmosToggle(InputAction.CallbackContext context)
    {
        if (!context.started) return; // Button down
        var torsionalOscillators = GameObject.FindObjectsOfType<TorsionalOscillator>();

        var baseAround = torsionalOscillators[0].renderGizmos;
        foreach (var torsionalOscillator in torsionalOscillators)
        {
            torsionalOscillator.renderGizmos = !baseAround;
        }
    }

    /// <summary>
    /// Toggles the gizmos belonging to all Oscillators.
    /// </summary>
    /// <param name="context">The input's context.</param>
    public void OscillatorGizmosToggle(InputAction.CallbackContext context)
    {
        if (!context.started) return; // Button down
        var oscillators = GameObject.FindObjectsOfType<Oscillator>();

        var baseAround = oscillators[0].renderGizmos;
        foreach (var oscillator in oscillators)
        {
            if (oscillator != playerOscillator)
            {
                oscillator.renderGizmos = !baseAround;
            }
        }
    }

    /// <summary>
    /// Toggles the gizmos belonging to the Player oscillator.
    /// </summary>
    /// <param name="context">The input's context.</param>
    public void PlayerGizmosToggle(InputAction.CallbackContext context)
    {
        if (context.started) // Button down
        {
            playerOscillator.renderGizmos = !playerOscillator.renderGizmos;
        }
    }

    /// <summary>
    /// Toggles the gizmos belonging to all Oscillators and Torsional Oscillators.
    /// </summary>
    /// <param name="context">The input's context.</param>
    public void GizmosToggle(InputAction.CallbackContext context)
    {
        if (!context.started) return; // Button down
        // Toggle gizmo drawing
        //Gizmos.Enabled = !Gizmos.Enabled;
        var oscillators = GameObject.FindObjectsOfType<Oscillator>();
        var torsionalOscillators = GameObject.FindObjectsOfType<TorsionalOscillator>();

        var baseAround = oscillators[0].renderGizmos;
        foreach (var oscillator in oscillators)
        {
            oscillator.renderGizmos = !baseAround;
        }
        foreach (var torsionalOscillator in torsionalOscillators)
        {
            torsionalOscillator.renderGizmos = !baseAround;
        }
    }
}
