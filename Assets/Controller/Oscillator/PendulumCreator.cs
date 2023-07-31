using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sequentially creates a row of pre-defined pendulums.
/// </summary>
public class PendulumCreator : MonoBehaviour
{
    [SerializeField] private GameObject pendulumPrefab;

    public int numPendulums = 1;

    [SerializeField] private float angle = 20f;

    public float incrementalTime;

    public List<GameObject> pendulums = new List<GameObject>();

    [HideInInspector] public Vector3 displacement;

    [HideInInspector] public float popInDistance = 10f; 

    /// <summary>
    /// Instantiate all the pendulums, to which a scale of zero is given.
    /// </summary>
    private void Start()
    {
        _t = new List<float>(new float[numPendulums]);
        _prevLerp = new List<float>(new float[numPendulums]);

        var incrementally = incrementalTime != 0f;

        displacement = transform.position;
        if (incrementally)
        {
            displacement += Vector3.left * popInDistance;
        }
        for (var i = 0; i < numPendulums; i++)
        {
            displacement += Vector3.left * 3f;
            var pendulum = Instantiate(pendulumPrefab, displacement, Quaternion.identity, transform);
            pendulums.Add(pendulum);
            if (incrementally)
            {
                pendulums[i].transform.localScale = Vector3.zero;
            }

            var torsionalOscillator = pendulum.GetComponent<TorsionalOscillator>();
            torsionalOscillator.stiffness *= (1f + 1f * i / numPendulums);

            torsionalOscillator.transform.localRotation = Quaternion.Euler(angle, 0, 0);
        }
    }

    public bool started;
    private List<float> _t;
    private List<float> _prevLerp;
    /// <summary>
    /// Increase the scales of the pendulums over time until they are the default scale, and place the pendulums in a row.
    /// </summary>
    private void Update()
    {
        for (var i = 0; i < numPendulums; i++)
        {
            if (!started)
            {
                if (i != 0)
                {
                    break;
                }
            }
            if (_t[i] < incrementalTime)
            {
                if (i > 0)
                {
                    if (_t[i - 1] >= incrementalTime)
                    {
                        _t[i] += Time.deltaTime / incrementalTime;
                        if (_t[i] >= incrementalTime)
                        {
                            _t[i] = incrementalTime;
                        }

                        var currLerp = Easing.Elastic.Out(_t[i]);
                        var deltaLerp = currLerp - _prevLerp[i];
                        pendulums[i].transform.localScale += Vector3.one * (deltaLerp * 2f);
                        pendulums[i].transform.position += Vector3.right * (deltaLerp * popInDistance);
                        _prevLerp[i] = currLerp;
                    }
                }
            
                else // i == 0
                {
                    _t[i] += Time.deltaTime / incrementalTime;
                    if (_t[i] >= incrementalTime)
                    {
                        _t[i] = incrementalTime;
                    }

                    var currLerp = Easing.Elastic.Out(_t[i]);
                    var deltaLerp = currLerp - _prevLerp[i];
                    pendulums[i].transform.localScale += Vector3.one * (deltaLerp * 2f);
                    pendulums[i].transform.position += Vector3.right * (deltaLerp * popInDistance);
                    _prevLerp[i] = currLerp;
                }
            }
        }
    }
}
