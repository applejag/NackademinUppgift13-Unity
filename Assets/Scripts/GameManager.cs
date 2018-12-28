using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleshipProtocol;
using BattleshipProtocol.Game;
using BattleshipProtocol.Game.Commands;
using BattleshipProtocol.Protocol;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameBoard board;
    public BoardVisualizer localVisualizer;
    public BoardVisualizer remoteVisualizer;

    [NonSerialized]
    public BattleGame game;
    private CancellationTokenSource connectCancellationToken;
    private CancellationTokenSource hostCancellationTokenSource;

    public async Task JoinGame(string address, ushort port, string localPlayerName)
    {
        connectCancellationToken = new CancellationTokenSource(millisecondsDelay: 10000);

        game = await BattleGame.ConnectAsync(new ConnectionSettings
        {
            Address = address,
            Port = port
        }, board.protocolBoard, localPlayerName, connectCancellationToken.Token);

        SetupEventHandlers();
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

        SetupEventHandlers();
        print("HOST CONNECTED WITH " + game.RemotePlayer.EndPoint);
    }

    public Task Fire(Coordinate coordinate)
    {
        return game.ShootAtAsync(coordinate, null);
    }

    private void SetupEventHandlers()
    {
        localVisualizer.board.SetBoard(game.LocalPlayer.Board);
        remoteVisualizer.board.SetBoard(game.RemotePlayer.Board);

        var fireCommand = game.PacketConnection.GetCommand<FireCommand>();

        fireCommand.TakenFire += (sender, outcome) => Dispatcher.Invoke(() =>
        {
            if (outcome.ShipHit is null)
                localVisualizer.PlaceMissAt(new Vector2Int(outcome.Coordinate.X, outcome.Coordinate.Y), outcome.ShipHit);
            else
                localVisualizer.PlaceHitAt(new Vector2Int(outcome.Coordinate.X, outcome.Coordinate.Y), outcome.ShipHit);
        });

        fireCommand.FireResponse += (sender, outcome) => Dispatcher.Invoke(() =>
        {
            if (outcome.ShipHit is null)
                remoteVisualizer.PlaceMissAt(new Vector2Int(outcome.Coordinate.X, outcome.Coordinate.Y), outcome.ShipHit);
            else
                remoteVisualizer.PlaceHitAt(new Vector2Int(outcome.Coordinate.X, outcome.Coordinate.Y), outcome.ShipHit);
        });

        foreach (Ship ship in game.RemotePlayer.Board.Ships)
        {
            ship.ShipMoved += delegate { Dispatcher.Invoke(remoteVisualizer.OnShipMoved, ship); };
        }
        foreach (Ship ship in game.LocalPlayer.Board.Ships)
        {
            ship.ShipMoved += delegate { Dispatcher.Invoke(localVisualizer.OnShipMoved, ship); };
        }
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
