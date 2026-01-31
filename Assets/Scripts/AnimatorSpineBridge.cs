using UnityEngine;

public class AnimatorSpineBridge : MonoBehaviour
{
    public Animator animator;
    public SpineTrackController spine;

    int lastStage = -999;
    bool lastMoving;
    bool lastIsBack;

    float lastX, lastY;

    void Update()
    {
        bool isMoving = animator.GetBool("IsMoving");
        float moveX = animator.GetFloat("MoveX");
        spine.SetFacing(moveX);
        float moveY = animator.GetFloat("MoveY");

        int stage = animator.GetInteger("Stage");
        bool isBack = animator.GetBool("IsBack");

        // Locomotion (body)
        if (isMoving != lastMoving || moveX != lastX || moveY != lastY)
        {
            spine.SetLocomotion(moveX, moveY, isMoving);
            lastMoving = isMoving;
            lastX = moveX;
            lastY = moveY;
        }

        // Head mode (stage + backstage)
        if (stage != lastStage || isBack != lastIsBack)
        {
            spine.SetHeadMode(stage, isBack);
            lastStage = stage;
            lastIsBack = isBack;
        }
    }
}
