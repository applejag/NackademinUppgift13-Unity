using System.Collections;
using System.Collections.Generic;
using BattleshipProtocol;
using BattleshipProtocol.Game;
using UnityEngine;
using UnityEngine.UI;

public class BoardVisualizer : MonoBehaviour
{
    public Transform localGridParent;
    public Transform remoteGridParent;

    public Color idleColor = new Color(1,1,1,0.5f);
    public Color missColor = Color.black;
    public Color hitColor = Color.red;
    public Color shipColor = Color.green;

    [SerializeField] private Image[,] localGrid;
    [SerializeField] private Image[,] remoteGrid;

    private void Awake()
    {
        localGrid = GetGrid(localGridParent);
        remoteGrid = GetGrid(remoteGridParent);
    }

    private Image[,] GetGrid(Transform parent)
    {
        var grid = new Image[10, 10];

        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                grid[x,y] = parent.GetChild(x + y * 10).GetComponent<Image>();
            }
        }

        return grid;
    }

    private void VisualizeGrid(Board board, Image[,] images)
    {
        if (board is null)
        {
            foreach (Image image in images)
            {
                image.color = Color.clear;
            }

            return;
        }

        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                bool ship = board.GetShipAt((x, y)) != null;
                bool shot = board.IsShotAt((x, y));

                Image image = images[x, y];

                if (ship && shot)
                    image.color = hitColor;
                else if (ship)
                    image.color = shipColor;
                else if (shot)
                    image.color = missColor;
                else
                    image.color = idleColor;
            }
        }
    }

    public void UpdateTheGrids(BattleGame game)
    {
        VisualizeGrid(game?.LocalPlayer.Board, localGrid);
        VisualizeGrid(game?.RemotePlayer.Board, remoteGrid);
    }
}
