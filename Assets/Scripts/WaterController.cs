using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public Animator animator;
    [Space]
    public new AnimationClip animation;
    public float speed = 1;

    [Range(0, 1)]
    public float targetTime;

    public string parameter = "Expanded";

#if UNITY_EDITOR
    [SerializeField, Range(0, 1), Header("DEBUGGING")]
    private float oldTime;

    private void OnValidate()
    {
        if (animator && animator.layerCount > 0 && animation == null)
        {
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfos.Length > 0)
                animation = clipInfos[0].clip;
        }
        if (animation)
        {
            speed = 1/animation.length;
        }
    }
#endif

    private void Update()
    {
#if !UNITY_EDITOR
        float oldTime;
#endif
        oldTime = animator.GetFloat(parameter);
        float newTime = Mathf.MoveTowards(oldTime, targetTime, speed * Time.deltaTime);
        animator.SetFloat(parameter, newTime);
    }

    public void SetTargetTime(float time)
    {
        targetTime = Mathf.Clamp01(time);
    }
}
