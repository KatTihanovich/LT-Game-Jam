using UnityEngine;
using Spine.Unity;

public class SpineTrackController : MonoBehaviour
{
    SkeletonAnimation sa;

    [Header("Track 0 (body locomotion)")]
    public string idle = "Idle";
    public string walk = "Walk";
    public string walkSide = "Walk_Side";

    [Header("Track 1 (head stage overlay)")]
    public string stage1 = "Stage_1_IS_ON";
    public string stage2 = "Stage_2_IS_ON";
    public string stage3 = "Stage_3_IS_ON";

    public string backstage1 = "Back_Stage_1_IS_ON";
    public string backstage2 = "Back_Stage_2_IS_ON";
    public string backstage3 = "Back_Stage_3_IS_ON";

    [Header("Track 2 (extra)")]
    public string blink = "Blink";

    [Header("Track 3 (one-shots)")]
    public string pulledUp = "Pulled_Up";

    [Header("Special")]
    public string stage3MaskRemove = "Stage_3_Mask_Remove";

    string currentBodyAnim;
    string currentHeadAnim;

    void Awake()
    {
        sa = GetComponent<SkeletonAnimation>();
        if (!sa)
        {
            Debug.LogError("No SkeletonAnimation on this GameObject.");
            enabled = false;
        }
    }

    void Start()
    {
        PlayBody(idle);
        PlayHead(stage1);
        sa.AnimationState.SetAnimation(2, blink, true);
    }

    public void SetLocomotion(float moveX, float moveY, bool isMoving)
    {
        string target;

        if (!isMoving) target = idle;
        else
        {
            bool side = Mathf.Abs(moveX) > Mathf.Abs(moveY);
            target = side ? walkSide : walk;
        }

        PlayBody(target);
    }

    public void SetHeadMode(int stage, bool isBack)
    {
        stage = Mathf.Clamp(stage, 1, 3);

        // Choose target animation name
        string target =
            isBack
                ? (stage == 1 ? backstage1 : stage == 2 ? backstage2 : backstage3)
                : (stage == 1 ? stage1     : stage == 2 ? stage2     : stage3);

        // Special: front Stage 3 has mask-remove intro
        if (!isBack && stage == 3)
        {
            // Always restart the sequence, even if already in stage3
            sa.AnimationState.SetEmptyAnimation(1, 0f);
            sa.AnimationState.SetAnimation(1, stage3MaskRemove, false);
            sa.AnimationState.AddAnimation(1, stage3, true, 0f);

            currentHeadAnim = stage3; // keep your "current" tracking consistent
            return;
        }

        // All other cases
        PlayHead(target);
    }

    void PlayBody(string animName)
    {
        if (currentBodyAnim == animName) return;
        currentBodyAnim = animName;
        sa.AnimationState.SetAnimation(0, animName, true);
    }

    void PlayHead(string animName)
    {
        if (currentHeadAnim == animName) return;
        currentHeadAnim = animName;
        sa.AnimationState.SetAnimation(1, animName, true);
    }

    public void PulledUp()
    {
        // Play on highest track so it overrides head + body
        sa.AnimationState.SetAnimation(3, pulledUp, false);

        // After it finishes, clear the track so body/head continue
        sa.AnimationState.AddEmptyAnimation(3, 0.1f, 0f);
    }

    public void SetFacing(float moveX)
    {
        if (!sa) return;

        // Only flip when we actually have direction
        if (Mathf.Abs(moveX) < 0.001f) return;

        float sign = Mathf.Sign(moveX);        // +1 right, -1 left
        sa.Skeleton.ScaleX = Mathf.Abs(sa.Skeleton.ScaleX) * sign;
        sa.Skeleton.UpdateWorldTransform();
    }

}
