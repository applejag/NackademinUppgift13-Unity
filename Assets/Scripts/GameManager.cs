using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BattleshipProtocol;
using BattleshipProtocol.Protocol;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameBoard board;

    [NonSerialized]
    public BattleGame game;
    private CancellationTokenSource connectCancellationToken;
    private CancellationTokenSource hostCancellationTokenSource;

    public async Task JoinGame(string address, ushort port, string localPlayerName)
    {
        connectCancellationToken = new CancellationTokenSource();

        game = await BattleGame.ConnectAsync(new ConnectionSettings
        {
            Address = address,
            Port = port
        }, board.protocolBoard, localPlayerName, connectCancellationToken.Token);

        print("CLIENT CONNECTED WITH " + game.RemotePlayer.EndPoint);
    }

    public void CancelConnect()
    {
        connectCancellationToken?.Cancel();
        connectCancellationToken = null;
        hostCancellationTokenSource?.Cancel();
        hostCancellationTokenSource = null;
        game?.Dispose();
        game = null;
    }

    public async Task HostGame(ushort port, string localPlayerName)
    {
        if (hostCancellationTokenSource != null)
            throw new InvalidOperationException("Game is already hosting.");

        hostCancellationTokenSource = new CancellationTokenSource();

        game = await BattleGame.HostAndWaitAsync(new ConnectionSettings
        {
            Port = port
        }, board.protocolBoard, localPlayerName, hostCancellationTokenSource.Token);

        print("HOST CONNECTED WITH " + game.RemotePlayer.EndPoint);
    }

    private void OnEnable()
    {
        CultureInfo.DefaultThreadCurrentCulture =
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
    }

    private void OnDisable()
    {
        connectCancellationToken?.Dispose();
        connectCancellationToken = null;
        game?.Disconnect();
        game?.Dispose();
        game = null;
    }
}
