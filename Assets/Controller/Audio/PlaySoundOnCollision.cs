using UnityEngine;

/// <summary>
/// Play a given sound when this rigid body / collider collides.
/// </summary>
public class PlaySoundOnCollision : MonoBehaviour
{
    public string audioName;

    /// <summary>
    /// Play the sound when this rigid body / collider collides with another.
    /// </summary>
    private void OnCollisionEnter()
    {
        FindObjectOfType<AudioManager>().Play(audioName);
    }
}
