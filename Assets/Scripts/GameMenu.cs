using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("Game manager stuff")]
    public string localPlayerName;

    [Header("Settings")]
    public float fadeSpeed = 3;
    [Range(0, 1)]
    public float interactableThreshold = 0.9f;
    [Range(0, 1)]
    public float blockRaycastThreshold = 0.4f;
    [Range(0, 1)]
    public float fadingThreshold = 0.95f;

    [Header("CanvasGroup references")]
    public CanvasGroup groupBackground;

    public CanvasGroup groupPlayerNamePanel;
    public CanvasGroup groupPlayerNameLocal;
    public CanvasGroup groupPlayerNameRemote;
    public CanvasGroup groupPickName;
    public CanvasGroup groupPlaceYourShips;
    public CanvasGroup groupNewGame;
    public CanvasGroup groupHostGame;
    public CanvasGroup groupJoinGame;
    public CanvasGroup groupLoadingModalWithAbort;
    public CanvasGroup groupErrorModalWithClose;
    public CanvasGroup groupGameStarted;

    [Header("Input references")]
    public InputField fieldPlayerName;
    public InputField fieldHostPort;
    public InputField fieldJoinAddress;
    public InputField fieldJoinPort;

    [Header("Specific elements")]
    public Text textPlayerName;
    public Button buttonFinishMovingShips;
    public Text textErrorMessage;

    [SerializeField, HideInInspector]
    private HashSet<CanvasGroup> currentlyFading = new HashSet<CanvasGroup>();

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FadeInMenuCoroutine(groupBackground));
        yield return new WaitForSeconds(1);
        StartCoroutine(FadeInMenuCoroutine(groupPickName));
    }

    public void FadeInMenu(CanvasGroup canvasGroup)
    {
        StartCoroutine(FadeInMenuCoroutine(canvasGroup));
    }
    public void FadeOutMenu(CanvasGroup canvasGroup)
    {
        StartCoroutine(FadeOutMenuCoroutine(canvasGroup));
    }

    private bool TryLock(CanvasGroup canvasGroup)
    {
        return currentlyFading.Add(canvasGroup);
    }

    private bool Unlock(CanvasGroup canvasGroup)
    {
        return currentlyFading.Remove(canvasGroup);
    }

    private IEnumerator FadeInMenuCoroutine(CanvasGroup canvasGroup)
    {
        while (!TryLock(canvasGroup))
            yield return null;

        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeInStart.Invoke();
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;

        while ((canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1, Time.deltaTime * fadeSpeed)) < fadingThreshold)
        {
            canvasGroup.interactable = canvasGroup.alpha > interactableThreshold;
            yield return null;
        }

        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeInEnd.Invoke();

        Unlock(canvasGroup);
    }

    private IEnumerator FadeOutMenuCoroutine(CanvasGroup canvasGroup)
    {
        while (!TryLock(canvasGroup))
            yield return null;

        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeOutStart.Invoke();
        canvasGroup.interactable = false;
        while ((canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, Time.deltaTime * fadeSpeed)) > 1 - fadingThreshold)
        {
            canvasGroup.blocksRaycasts = canvasGroup.alpha > blockRaycastThreshold;
            yield return null;
        }
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeOutEnd.Invoke();

        Unlock(canvasGroup);
    }

    public void Menu_PickName_SetPlayerName()
    {
        string playerName = fieldPlayerName.text.Trim();
        if (playerName.Length > 0)
        {
            textPlayerName.text = localPlayerName = playerName;
            FadeOutMenu(groupBackground);
            FadeOutMenu(groupPickName);
            FadeInMenu(groupPlaceYourShips);
            FadeInMenu(groupPlayerNamePanel);
            FadeInMenu(groupPlayerNameLocal);
        }
        else
        {
            Menu_Error_Show("Please enter your name.");
        }
    }

    public void Menu_PickName_CancelSetName()
    {
        if (localPlayerName.Length == 0)        
        {
            Menu_Error_Show("Please enter your name.");
        }
        else
        {
            FadeOutMenu(groupPickName);
        }
    }

    public void Menu_PlaceYourShips_AllShipsMoved()
    {
        buttonFinishMovingShips.interactable = true;
    }

    public void Menu_PlaceYourShips_FinishedButton()
    {
        FadeOutMenu(groupPlaceYourShips);
        FadeInMenu(groupNewGame);
    }

    public void Menu_Error_Show(string error)
    {
        textErrorMessage.text = error;
        FadeInMenu(groupErrorModalWithClose);
    }
}
