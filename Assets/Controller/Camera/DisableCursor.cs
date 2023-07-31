using UnityEngine;

/// <summary>
/// Disable cursor visibility during Play mode.
/// </summary>
public class DisableCursor : MonoBehaviour
{
    /// <summary>
    /// Disable cursor visibility when playing.
    /// </summary>
    private void Start()
    {
        Cursor.visible = false;
    }
}
