using UnityEngine;

/// <summary>
/// A spawn effect for an object which scales up the object with a bounce, plays a sound, and activates particle systems.
/// </summary>
[System.Serializable]
public class Effect : MonoBehaviour
{
    public ParticleSystem[] particleSystems;

    public GameObject activatedGameObject;
    public Vector3 startPos;
    public Vector3 endPos;
    public Vector3 endScale;
    public bool isPlaying;

    /// <summary>
    /// Plays a sound effect for spawning a game object, and activates particle effects.
    /// </summary>
    /// <param name="anchor">The transform about which the effects should take place.</param>
    /// <param name="activatedObject">The game object to spawn.</param>
    public void ActivateEffects(Transform anchor, GameObject activatedObject)
    {
        _t = 0f;
        activatedGameObject = activatedObject;
        startPos = anchor.position;
        endPos = activatedObject.transform.position;
        endScale = activatedObject.transform.localScale;
        isPlaying = true;    

        FindObjectOfType<AudioManager>().Play("Woo");

        SetParent(anchor);

        foreach (var system in particleSystems)
        {
            system.Play();
            var emission = system.emission;
            emission.enabled = true;
        }
    }

    /// <summary>
    /// Updates the scale of the game object when appropriate.
    /// </summary>
    private void Update()
    {
        if (isPlaying)
        {
            ScaleUp(startPos, endPos, endScale);
        }
    }

    /// <summary>
    /// Lerps the scale and position of the spawned game object, with a bounce.
    /// </summary>
    private float _t;
    private void ScaleUp(Vector3 startPosition, Vector3 endPosition, Vector3 endScaleEffect, float duration = 0.7f)
    {
        _t += Time.deltaTime / duration;
        _t = Mathf.Min(_t, 1f);

        var t = Easing.Bounce.Out(_t);

        // Lerp position from startPosition to endPosition
        var position = MathsUtils.LerpVector3(startPosition, endPosition, t);

        // Lerp scale from Vector3.zero to endScaleEffect
        var scale = MathsUtils.LerpVector3(Vector3.zero, endScaleEffect, t);

        activatedGameObject.transform.position = position;
        activatedGameObject.transform.localScale = scale;

        if (_t >= 1f)
        {
            isPlaying = false;
        }
    }

    /// <summary>
    /// Sets the transform parent of the particle systems, such as to recycle a single particle system over multiple effects.
    /// </summary>
    /// <param name="parent">The anchor transform about which the particle systems should occur.</param>
    private void SetParent(Transform parent)
    {
        foreach (var system in particleSystems)
        {
            Transform transform1;
            (transform1 = system.transform).SetParent(parent, true);
            transform1.localPosition = Vector3.zero;
            transform1.localScale = Vector3.one;
        }
    }
}
