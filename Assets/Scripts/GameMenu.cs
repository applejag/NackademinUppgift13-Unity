using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BattleshipProtocol;
using BattleshipProtocol.Protocol;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("Game manager stuff")]
    public string localPlayerName;

    public GameManager gameManager;

    [Header("Settings")]
    public string joiningGameText = "CONNECTING…";
    public string waitingForPlayerText = "WAITING FOR PLAYER…";
    public string waitingForHandshakeText = "WAITING FOR HANDSHAKE…";
    public string waitingForStartText = "WAITING FOR CLIENT TO START…";
    public string waitingForTurnText = "WAITING FOR HOST TO PICK TURN…";
    public string ourTurnText = "YOUR TURN";
    public string theirTurnText = "THEIR TURN";
    [Space]
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
    public CanvasGroup groupJoinClientStartGame;
    public CanvasGroup groupLoadingModalWithAbort;
    public CanvasGroup groupErrorModalWithClose;
    public CanvasGroup groupGameStarted;
    public CanvasGroup groupDisconnected;

    [Header("Input references")]
    public InputField fieldPlayerName;
    public InputField fieldHostPort;
    public InputField fieldJoinAddress;
    public InputField fieldJoinPort;

    [Header("Specific elements")]
    [FormerlySerializedAs("textPlayerName")]
    public Text textPlayerLocalName;
    public Text textPlayerRemoteName;
    public Button buttonFinishMovingShips;
    public Text textErrorMessage;
    public Text textLoadingHeader;
    public Button buttonLoadingAbort;
    public Text textGameStartedTurn;

    private Dictionary<CanvasGroup, CancellationTokenSource> currentlyFading = new Dictionary<CanvasGroup, CancellationTokenSource>();

    private bool connectCancelByTimeout = true;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FadeInMenuCoroutine(groupBackground, fadeSpeed * 0.5f, GetTokenAndCancel(groupBackground)));
        yield return new WaitForSeconds(1);
        StartCoroutine(FadeInMenuCoroutine(groupPickName, fadeSpeed, GetTokenAndCancel(groupPickName)));
    }

    public void FadeInMenu(CanvasGroup canvasGroup)
    {
        StartCoroutine(FadeInMenuCoroutine(canvasGroup, fadeSpeed, GetTokenAndCancel(canvasGroup)));
    }

    public void FadeOutMenu(CanvasGroup canvasGroup)
    {
        StartCoroutine(FadeOutMenuCoroutine(canvasGroup, fadeSpeed, GetTokenAndCancel(canvasGroup)));
    }

    private CancellationToken GetTokenAndCancel(CanvasGroup canvasGroup)
    {
        if (currentlyFading.TryGetValue(canvasGroup, out CancellationTokenSource oldTokenSource))
        {
            oldTokenSource.Cancel();
            oldTokenSource.Dispose();
        }

        var tokenSource = new CancellationTokenSource();
        currentlyFading[canvasGroup] = tokenSource;
        return tokenSource.Token;
    }

    private void DisposeMySource(CanvasGroup canvasGroup, CancellationToken myToken)
    {
        if (currentlyFading.TryGetValue(canvasGroup, out CancellationTokenSource oldTokenSource)
            && oldTokenSource.Token == myToken)
        {
            oldTokenSource.Cancel();
            oldTokenSource.Dispose();
            currentlyFading.Remove(canvasGroup);
        }
    }

    private IEnumerator FadeInMenuCoroutine(CanvasGroup canvasGroup, float speed, CancellationToken cancellationToken)
    {
        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeInStart.Invoke();
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;

        while (!cancellationToken.IsCancellationRequested &&
               (canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1, Time.deltaTime * speed)) < fadingThreshold)
        {
            canvasGroup.interactable = canvasGroup.alpha > interactableThreshold;
            yield return null;
        }

        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeInEnd.Invoke();

        DisposeMySource(canvasGroup, cancellationToken);
    }

    private IEnumerator FadeOutMenuCoroutine(CanvasGroup canvasGroup, float speed, CancellationToken cancellationToken)
    {
        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeOutStart.Invoke();
        canvasGroup.interactable = false;
        while (!cancellationToken.IsCancellationRequested &&
               (canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, Time.deltaTime * speed)) > 1 - fadingThreshold)
        {
            canvasGroup.blocksRaycasts = canvasGroup.alpha > blockRaycastThreshold;
            yield return null;
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
        canvasGroup.GetComponent<MenuGroupEvents>()?.OnFadeOutEnd.Invoke();

        DisposeMySource(canvasGroup, cancellationToken);
    }

    #region Menu button callbacks

    public void Menu_PickName_SetPlayerName()
    {
        string playerName = fieldPlayerName.text.Trim();
        if (playerName.Length > 0)
        {
            textPlayerLocalName.text = localPlayerName = playerName;
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

    public void Menu_Error_Show(Exception error, string errorTitle)
    {
        switch (error)
        {
            case null:
                return;
            case AggregateException multiple:
                List<Exception> innerExceptions = multiple.InnerExceptions.ToList();
                if (innerExceptions.Count == 1)
                {
                    error = innerExceptions[0];
                    goto default;
                }
                Menu_Error_Show($"{errorTitle}\n{string.Join("\n", innerExceptions.Select(e => "- " + e.Message))}");
                break;

            default:
                Menu_Error_Show($"{errorTitle}\n{error.Message}");
                break;
        }

        Debug.LogException(error, this);
    }

    public void Menu_Loading_Show(string header, bool withAbortButton = true)
    {
        textLoadingHeader.text = header;
        buttonLoadingAbort.gameObject.SetActive(withAbortButton);
        FadeInMenu(groupLoadingModalWithAbort);
    }

    public void Menu_JoinGame_Connect()
    {
        if (!TryGetAddressField(fieldJoinAddress, out string address)) return;
        if (!TryGetPortField(fieldJoinPort, out ushort port)) return;

        Menu_Loading_Show(joiningGameText);

        connectCancelByTimeout = true;
        gameManager.JoinGame(address, port, localPlayerName)
            .ContinueWith(task => Dispatcher.Invoke(HandleGameConnectResult, task, "Error while connecting to game:"));
    }

    public void Menu_JoinClientStart_Start()
    {
        Menu_Loading_Show(waitingForTurnText, withAbortButton: false);

        gameManager.game.StartGameAsync()
            .ContinueWith(Dispatcher.Wrap<Task>(task =>
            {
                FadeOutMenu(groupLoadingModalWithAbort);
                Menu_Error_Show(task.Exception, "Error while starting game:");
            }), TaskContinuationOptions.OnlyOnFaulted);
    }

    public void Menu_Loading_Abort()
    {
        connectCancelByTimeout = false;
        gameManager.CancelConnect();
        FadeOutMenu(groupLoadingModalWithAbort);
    }

    public void Menu_HostGame_Host()
    {
        if (!TryGetPortField(fieldHostPort, out ushort port)) return;

        Menu_Loading_Show(waitingForPlayerText);

        connectCancelByTimeout = true;
        gameManager.HostGame(port, localPlayerName)
            .ContinueWith(task => Dispatcher.Invoke(HandleGameConnectResult, task, "Error while hosting game:"));
    }

    public void Menu_Disconnected_Dismiss()
    {
        gameManager.CancelConnect();
        
        FadeOutMenu(groupPlayerNameRemote);
        FadeOutMenu(groupNewGame);
        FadeOutMenu(groupHostGame);
        FadeOutMenu(groupJoinGame);
        FadeOutMenu(groupJoinClientStartGame);
        FadeOutMenu(groupLoadingModalWithAbort);
        FadeOutMenu(groupErrorModalWithClose);
        FadeOutMenu(groupGameStarted);
        FadeOutMenu(groupDisconnected);

        FadeInMenu(groupPlaceYourShips);
    }

    #endregion

    private void HandleGameConnectResult(Task task, string errorTitle)
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            FadeOutMenu(groupLoadingModalWithAbort);
            
            if (task.Exception != null)
            {
                Menu_Error_Show(task.Exception, errorTitle);
                gameManager.CancelConnect();
            }
            else if (connectCancelByTimeout)
            {
                Menu_Error_Show("Connection timed out.");
            }

            return;
        }

        gameManager.game.GameStateChanged += delegate { Dispatcher.Invoke(HandleGameState, gameManager.game); };
        gameManager.game.RemotePlayer.NameChanged += delegate { Dispatcher.Invoke(() =>
        {
            textPlayerRemoteName.text = gameManager.game.RemotePlayer.Name;
            FadeInMenu(groupPlayerNameRemote);
        }); };

        HandleGameState(gameManager.game);
    }

    public void HandleGameState(BattleGame game)
    {
        switch (game.GameState)
        {
            case GameState.Handshake:
                Menu_Loading_Show(waitingForHandshakeText);
                break;

            case GameState.Idle when game.IsHost:
                Menu_Loading_Show(waitingForStartText);
                break;

            case GameState.Idle when game.IsHost == false:
                FadeInMenu(groupJoinClientStartGame);
                FadeOutMenu(groupHostGame);
                FadeOutMenu(groupJoinGame);
                FadeOutMenu(groupLoadingModalWithAbort);
                break;

            case GameState.InGame:
                FadeInMenu(groupGameStarted);
                textGameStartedTurn.text = game.IsLocalsTurn ? ourTurnText : theirTurnText;
                FadeOutMenu(groupJoinGame);
                FadeOutMenu(groupJoinClientStartGame);
                FadeOutMenu(groupHostGame);
                FadeOutMenu(groupLoadingModalWithAbort);
                break;

            case GameState.Disconnected:
                FadeInMenu(groupDisconnected);
                break;
        }
    }

    private bool TryGetAddressField(InputField field, out string address)
    {
        address = string.Empty;
        string input = field.text.Trim();

        if (string.IsNullOrEmpty(input))
        {
            Menu_Error_Show("The address field is required.");
            return false;
        }

        address = input;
        return true;
    }

    private bool TryGetPortField(InputField field, out ushort port)
    {
        port = default(ushort);
        string input = field.text.Trim();

        if (string.IsNullOrEmpty(input))
        {
            Menu_Error_Show("The port field is required.");
            return false;
        }

        if (!int.TryParse(input, out int result))
        {
            Menu_Error_Show($"Sorry, but \"{result}\" is an invalid format for port.\nOnly digits allowed.");
            return false;
        }

        if (result > ushort.MaxValue)
        {
            Menu_Error_Show($"Sorry, but port can be at max {ushort.MaxValue}.");
            return false;
        }

        if (result < 0)
        {
            Menu_Error_Show($"Sorry, but port cannot be negative. Maybe try {-result}");
            return false;
        }

        if (result == 0)
        {
            Menu_Error_Show("Sorry, but port must be bigger than 0.");
            return false;
        }

        port = (ushort)result;
        return true;
    }
}
