using UnityEngine;

/// <summary>
/// Linearly interpolate the camera position over time to follow a series of pendulums.
/// </summary>
public class LerpCamera : MonoBehaviour
{
    [SerializeField] private PendulumCreator pendulumCreator;

    private Vector3 _start;
    private Vector3 _end;
    private float _duration;
    private float _t;
    private Vector3 _originalPosition;

    /// <summary>
    /// Declare the original position of the camera.
    /// </summary>
    private void Start()
    {
        _originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Lerp the camera, if the series of pendulums has begun creation.
    /// </summary>
    private void Update()
    {
        if (!pendulumCreator.started) return;
        _start = Vector3.zero;
        _end = pendulumCreator.displacement - pendulumCreator.popInDistance * Vector3.left - pendulumCreator.transform.position;
        _duration = pendulumCreator.numPendulums * pendulumCreator.incrementalTime;
        _t += Time.deltaTime / _duration;
        transform.localPosition = _originalPosition + MathsUtils.LerpVector3(_start, _end, _t);
    }
}
