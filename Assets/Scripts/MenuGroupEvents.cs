using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuGroupEvents : MonoBehaviour
{
    public UnityEvent OnFadeInStart;
    public UnityEvent OnFadeInEnd;
    public UnityEvent OnFadeOutStart;
    public UnityEvent OnFadeOutEnd;

#if UNITY_EDITOR
    [Header("Testing buttons")]
    public bool fakeFadeIn;
    public bool fakeFadeOut;

    private void Update()
    {
        if (fakeFadeIn)
        {
            fakeFadeIn = false;
            GetComponentInParent<GameMenu>().FadeInMenu(GetComponent<CanvasGroup>());
        }

        if (fakeFadeOut)
        {
            fakeFadeOut = false;
            GetComponentInParent<GameMenu>().FadeOutMenu(GetComponent<CanvasGroup>());
        }
    }
#endif
}
