using UnityEngine;

/// <summary>
/// Dynamically colours a pendulum depending on it's angular displacement.
/// </summary>
[RequireComponent(typeof(TorsionalOscillator))]
[RequireComponent(typeof(MeshRenderer))]
public class DynamicPendulumColour : MonoBehaviour
{
    private TorsionalOscillator _torsionalOscillator;
    private Renderer _meshRenderer;
    private Material _dynamicPendulumMaterial;

    /// <summary>
    /// Define the required variables.
    /// </summary>
    private void Start()
    {
        _torsionalOscillator = GetComponent<TorsionalOscillator>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _dynamicPendulumMaterial = _meshRenderer.material;
    }

    /// <summary>
    /// Update the colour of the pendulum, such that it is determined by the angular displacement of the pendulum.
    /// </summary>
    private void FixedUpdate()
    {
        var angle = _torsionalOscillator.angularDisplacementMagnitude;

        var color = Color.green;
        const float upperAmplitude = 20f; // Approximately the upper limit of the angle amplitude within regular use
        var ratio = angle / upperAmplitude;

        var r = 2f * (1f - Mathf.Clamp(ratio, 0.5f, 1f));
        var g = 2f * Mathf.Clamp(ratio, 0f, 0.5f);
        var b = 1f;

        color.r = r;
        color.g = g;
        color.b = b;
        Gizmos.color = color;

        _dynamicPendulumMaterial.color = color;
    }
}
