using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tracks and manages the current transition of the presentation.
/// </summary>
public class Presentation : MonoBehaviour
{
    public Transition[] transitions;

    public int step;

    /// <summary>
    /// Run the next transition.
    /// </summary>
    /// <param name="context">The input's context.</param>
    public void MakeNextTransition(InputAction.CallbackContext context)
    {
        if (!context.started) return; // button down
        var nextTransition = step + 1;
        nextTransition = Mathf.Min(nextTransition, transitions.Length);
        var success = transitions[nextTransition - 1].MakeTransition();
        if (!success)
        {
            return;
        }
        step = nextTransition;
    }

    /// <summary>
    /// Revert to the previous transition.
    /// </summary>
    /// <param name="context">The input's context.</param>
    public void RevertPrevTransition(InputAction.CallbackContext context)
    {
        if (!context.started) return; // button down
        var prevTransition = step - 1;
        prevTransition = Mathf.Max(0, prevTransition);
        var success = transitions[prevTransition].RevertTransition();
        if (!success)
        {
            return;
        }
        step = prevTransition;
    }
}
