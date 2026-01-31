using UnityEngine;

public class CallSpinePulledUpOnEnter : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Try to find SpineTrackController on the same GameObject or parents.
        var spine = animator.GetComponent<SpineTrackController>() ?? animator.GetComponentInParent<SpineTrackController>();
        if (spine != null)
            spine.PulledUp();
        else
            Debug.LogError("CallSpinePulledUpOnEnter: SpineTrackController not found on Animator object or parents.");
    }
}
