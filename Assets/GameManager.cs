using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using BattleshipProtocol;
using BattleshipProtocol.Game;
using BattleshipProtocol.Game.Commands;
using BattleshipProtocol.Protocol;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [FormerlySerializedAs("portField")]
    public InputField hostPortField;
    public InputField clientAddressField;
    public InputField clientPortField;
    public InputField nameField;
    public Text statusText;
    public Text logText;
    public Button hostButton;
    public Button connectButton;
    public Button dcButton;
    public Button startGameButton;
    public Button fireAtRandomButton;

    private BattleGame game;

    private readonly Board localBoard = new Board();

    private void OnEnable()
    {
        var x = 0;
        foreach (Ship ship in localBoard.Ships)
        {
            localBoard.MoveShip(ship.Type, (x++, 0), Orientation.South);
        }

        dcButton.interactable = false;
        logText.text = string.Empty;
        SetStatus();

        CultureInfo.DefaultThreadCurrentCulture =
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
    }

    private void SetStatus()
    {
        statusText.text = $"Game state: {game?.GameState}\n" +
                          $"Is host: {game?.IsHost}\n" +
                          $"Is locals turn: {game?.IsLocalsTurn}\n" +
                          $"Remote player: {game?.RemotePlayer.Name}\n" +
                          $"Remote endpoint: {game?.RemotePlayer.EndPoint}\n" +
                          $"Local player: {game?.LocalPlayer.Name}\n" +
                          $"Local endpoint: {game?.LocalPlayer.EndPoint}";

        fireAtRandomButton.interactable = game?.IsLocalsTurn == true;
        dcButton.interactable = game != null;
        startGameButton.interactable = game?.GameState == GameState.Idle;
    }

    public async void OnFireAtRandomButton()
    {
        try
        {
            if (game == null) return;


            Coordinate coordinate = new Coordinate();
            for (int i = 0; i < 50; i++)
            {
                coordinate = (Random.Range(0, 10), Random.Range(0, 10));
                if (!game.RemotePlayer.Board.IsShotAt(coordinate))
                    break;
                else if (i >= 49)
                {
                    throw new StackOverflowException("Couldnt find a coordinate to fire at...");
                }
            }

            await game.ShootAtAsync(coordinate, null);
            Log($"Firing at {coordinate}...", Color.gray);
        }
        catch (Exception e)
        {
            Log(e.Message, Color.red);
            Debug.LogException(e);
        }
    }

    public async void OnConnectButton()
    {
        try
        {
            game?.Dispose();

            hostPortField.interactable = false;
            clientAddressField.interactable = false;
            clientPortField.interactable = false;

            nameField.interactable = false;
            hostButton.interactable = false;
            connectButton.interactable = false;

            ushort port = ushort.Parse(clientPortField.text);
            string address = clientAddressField.text.Trim();
            Log($"Connecting to {address}:{port}...", Color.gray);

            game = await BattleGame.ConnectAsync(address, port, localBoard, nameField.text);

            Dispatcher.Invoke(SetStatus);

            Log($"Game connected with {game.RemotePlayer.EndPoint}", Color.green);
            HookupGameEvents();
        }
        catch (SocketException se)
        {
            DisposeTheGame();

            Log(se.Message, Color.red);
            Debug.LogError($"{(int)se.SocketErrorCode} {se.SocketErrorCode}");
            Debug.LogException(se);
        }
        catch (Exception e)
        {
            DisposeTheGame();

            Log(e.Message, Color.red);
            Debug.LogException(e);
        }
    }

    public async void OnHostButton()
    {
        try
        {
            game?.Dispose();

            hostPortField.interactable = false;
            clientAddressField.interactable = false;
            clientPortField.interactable = false;

            nameField.interactable = false;
            hostButton.interactable = false;
            connectButton.interactable = false;

            ushort port = ushort.Parse(hostPortField.text);
            Log($"Starting to listen on port {port}...", Color.gray);

            game = await BattleGame.HostAndWaitAsync(port, localBoard, nameField.text);

            Dispatcher.Invoke(SetStatus);

            Log($"Game connected with {game.RemotePlayer.EndPoint}", Color.green);
            HookupGameEvents();
        }
        catch (Exception e)
        {
            DisposeTheGame();

            Log(e.Message, Color.red);
            Debug.LogException(e);
        }
    }

    public async void OnStartGameButton()
    {
        try
        {
            if (game != null) await game?.StartGameAsync();
        }
        catch (Exception e)
        {
            Log(e.Message, Color.red);
            Debug.LogException(e);
        }
    }

    private void HookupGameEvents()
    {
        game.LocalPlayer.NameChanged += delegate
        {
            Dispatcher.Invoke(SetStatus);
            Log($"Local player name changed to {game.LocalPlayer.Name}", Color.gray);
        };
        game.RemotePlayer.NameChanged += delegate
        {
            Dispatcher.Invoke(SetStatus);
            Log($"Remote player name changed to {game.RemotePlayer.Name}", Color.gray);
        };
        game.GameStateChanged += delegate
        {
            Dispatcher.Invoke(SetStatus);
            Log($"Game state changed to {game.GameState}", Color.yellow);
            if (game.GameState == GameState.Disconnected)
            {
                DisposeTheGame();
            }
        };
        game.LocalsTurnChanged += delegate
        {
            Dispatcher.Invoke(SetStatus);
            Log($"Turn changed to {(game.IsLocalsTurn ? "local" : "remote")} player",
                game.IsLocalsTurn ? Color.green : Color.red);
        };

        var fireCommand = game.PacketConnection.GetCommand<FireCommand>();
        fireCommand.TakenFireMessage += delegate (object o, string msg)
        {
            Log($"Remote player said: <b>{msg}</b>", Color.yellow);
        };
        fireCommand.TakenFire += delegate (object sender, FireOutcome outcome)
        {
            Log(
                $"Took fire at {outcome.Coordinate}, {(outcome.ShipHit is null ? "miss!" : (outcome.ShipSunk ? "sunk" : "hit") + outcome.ShipHit.Name + "!")}",
                outcome.ShipHit is null ? Color.white : (outcome.ShipSunk ? Color.red : Color.yellow));
        };

        fireCommand.FireResponse += delegate (object sender, FireOutcome outcome)
        {
            Log(
                $"Our fire at {outcome.Coordinate}, {(outcome.ShipHit is null ? "missed :(" : (outcome.ShipSunk ? "sunk" : "hit") + outcome.ShipHit.Name + "!")}",
                outcome.ShipHit is null ? Color.white : (outcome.ShipSunk ? Color.green : Color.cyan));
        };
    }

    public void OnDisconnectButton()
    {
        Log($"Game disconnected by user.", Color.red);
        DisposeTheGame();
    }

    private async void DisposeTheGame()
    {
        if (game != null) await game.Disconnect();
        game = null;

        Dispatcher.Invoke(delegate
        {
            dcButton.interactable = false;
            hostButton.interactable = true;
            hostPortField.interactable = true;
            nameField.interactable = true;
            clientAddressField.interactable = true;
            clientPortField.interactable = true;
            connectButton.interactable = true;

            SetStatus();
        });
    }

    private void Log(string thing, Color color)
    {
        string Hex(float c)
        {
            return ((int)(c * 255)).ToString("x2");
        }
        Dispatcher.Invoke(delegate
        {
            logText.text = $"<color=#{Hex(color.r)}{Hex(color.g)}{Hex(color.b)}>{thing}</color>\n" + logText.text;
        });
    }

    private void OnDestroy()
    {
        game?.Dispose();
    }
}
