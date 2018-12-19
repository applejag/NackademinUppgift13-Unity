using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public float fadeSpeed = 3;
    public float interactableThreshold = 0.9f;
    public float blockRaycastThreshold = 0.4f;

    public CanvasGroup groupBackground;

    public List<CanvasGroup> subMenus;

    [SerializeField, HideInInspector]
    private CanvasGroup current;

    private void Update()
    {
        groupBackground.alpha = Mathf.Lerp(groupBackground.alpha, current == null ? 0 : 1, Time.deltaTime * fadeSpeed);
        groupBackground.blocksRaycasts = groupBackground.alpha > blockRaycastThreshold;

        foreach (CanvasGroup canvasGroup in subMenus)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, canvasGroup == current ? 1 : 0,
                Time.deltaTime * fadeSpeed);

            canvasGroup.interactable = canvasGroup.alpha > interactableThreshold;
            canvasGroup.blocksRaycasts = canvasGroup.alpha > blockRaycastThreshold;
        }
    }

    public void SetActiveMenu(string menu)
    {
        foreach (CanvasGroup canvasGroup in subMenus)
        {
            if (canvasGroup.name.Equals(menu, StringComparison.InvariantCultureIgnoreCase))
            {
                current = canvasGroup;
            }
        }
    }
}
