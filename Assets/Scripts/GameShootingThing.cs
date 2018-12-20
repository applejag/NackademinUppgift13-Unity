using System.Collections;
using System.Collections.Generic;
using BattleshipProtocol.Game;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

public class GameShootingThing : MonoBehaviour
{
    public GameBoard board;

    public string fireAtFormat = "FIRE ➵ {0}";
    public string fireAtNothingText = "SELECT TARGET";

    [Header("UI references")]
    public Button buttonFireButton;
    public Text textFireButton;

    [SerializeField, HideInInspector]
    private new Camera camera;

    [SerializeField, HideInInspector]
    private Vector2Int selectedCoordinate;

    [HideInInspector]
    public bool iWantTheFocus;

    private void Awake()
    {
        camera = Camera.main;
    }

    private void OnEnable()
    {
        textFireButton.text = fireAtNothingText;
        buttonFireButton.interactable = false;
        selectedCoordinate = new Vector2Int(-1, -1);
    }

    private void OnDisable()
    {
        iWantTheFocus = false;
    }

    public string FormatString(Vector2Int coordinate)
    {
        return Board.IsOnBoard(coordinate.x, coordinate.y)
            ? string.Format(fireAtFormat, new Coordinate(coordinate.x, coordinate.y).ToString())
            : fireAtNothingText;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            iWantTheFocus = false;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector2Int coordinate = board.ScreenSpaceToCoordinate(Input.mousePosition);
        if (!Board.IsOnBoard(coordinate.x, coordinate.y)) return;

        if (Input.GetMouseButtonDown(0))
            iWantTheFocus = true;

        selectedCoordinate = coordinate;
        textFireButton.text = FormatString(coordinate);
        buttonFireButton.interactable = true;
    }

    public void Fire()
    {
        if (Board.IsOnBoard(selectedCoordinate.x, selectedCoordinate.y))
        {
            
        }
    }
}
