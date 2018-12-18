﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipProtocol.Game;
using Extensions;
using UnityEngine;

public class BoardShipPlacer : MonoBehaviour
{
    public GameBoard board;
    public GameShip selectedShip;
    public Transform queuePivot;

    [HideInInspector]
    public List<GameShip> queueList;
    public float queueGap = 20;
    [HideInInspector]
    public float queueGapLeft;
    public float queueGapDescentSpeed = 2;

    public float raycastMaxDistance = 1000;

    [SerializeField, HideInInspector]
    private Camera cam;

    [SerializeField, HideInInspector]
    private Orientation dragOrientation = Orientation.South;

    private bool justSelected;

#if UNITY_EDITOR
    private List<GameShip> _editorQueueList;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.TransformVector(0, 0, 4 * queueGap + queueGapLeft));

        if (board == null)
            return;

        if (queueList != null && queueList.Count != 0)
        {
            GizmosForTheShips(queueList);
        }
        else if (!UnityEditor.EditorApplication.isPlaying)
        {
            GizmosForTheShips(_editorQueueList ?? (_editorQueueList = new List<GameShip>(board.ships)));
        }
    }

    private void GizmosForTheShips(List<GameShip> list)
    {
        Gizmos.color = Color.magenta;
        for (var i = 0; i < list.Count; i++)
        {
            ShipType shipType = list[i].shipType;
            var length = 2;

            if (Enum.IsDefined(typeof(ShipType), shipType))
                length = Ship.GetShipLength(shipType);

            Vector3 shipPosition = CalculateShipPosition(i);

            Vector3 size = transform.TransformVector(length, 0.1f, 1);
            Gizmos.DrawWireCube(shipPosition, size);
        }
    }
#endif

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        queueGapLeft = queueGap;
        queueList = new List<GameShip>(board.ships);
    }

    private void Update()
    {
        SlideTheQueue();
        SelectAShip();
        PlaceTheShip();
    }

    private void SelectAShip()
    {
        if (selectedShip != null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(mouseRay.origin, mouseRay.direction * raycastMaxDistance, Color.cyan, 1);
        if (!Physics.Raycast(mouseRay, out RaycastHit hitInfo, raycastMaxDistance))
            return;

        var gameShip = hitInfo.collider.GetComponent<GameShip>();

        if (gameShip is null)
            return;

        selectedShip = gameShip;
        dragOrientation = gameShip.GetShip().Orientation;
        justSelected = true;
    }

    private void PlaceTheShip()
    {
        if (selectedShip is null)
            return;

        Vector3 rayPosition = cam.ScreenToFlatWorldPoint(Input.mousePosition);
        Vector3 offset = selectedShip.GetPositionOffset();
        Vector2Int coordinate = board.WorldToCoordinate(rayPosition - offset);

        selectedShip.SetPositionFromCoordinate(coordinate, dragOrientation, 10);

        // Is whole ship outside?
        int length = selectedShip.GetLength();
        int xLength = dragOrientation == Orientation.East ? length : 1;
        int yLength = dragOrientation == Orientation.South ? length : 1;
        if (!Board.IsOnBoard(coordinate.x, coordinate.y) && !Board.IsOnBoard(coordinate.x + xLength - 1, coordinate.y + yLength - 1))
        {
            if (Input.GetMouseButtonDown(0))
                dragOrientation = FlipOrientation(dragOrientation);

            return;
        }

        if (!Input.GetMouseButtonUp(0))
            return;

        if (justSelected)
        {
            justSelected = false;
            return;
        }

        try
        {
            board.protocolBoard.MoveShip(selectedShip.shipType, (coordinate.x, coordinate.y), dragOrientation);
        }
        catch
        {
            return;
        }

        // Move queue
        if (queueList.Remove(selectedShip))
            queueGapLeft = queueGap;

        selectedShip.SetPositionFromCoordinate(coordinate, dragOrientation);
        print($"moved {selectedShip.name} to {coordinate}");
        selectedShip = null;
    }

    private void SlideTheQueue()
    {
        if (queueGapLeft > 0)
        {
            queueGapLeft = Mathf.Lerp(queueGapLeft, 0, Time.deltaTime * queueGapDescentSpeed);
            //queueGapLeft = Mathf.Max(queueGapLeft - Time.deltaTime * queueGapDescentSpeed, 0);
        }

        for (var index = 0; index < queueList.Count; index++)
        {
            queueList[index].transform.position = CalculateShipPosition(index);
        }
    }

    public Vector3 CalculateShipPosition(int index)
    {
        return queuePivot.TransformPoint(0, 0, queueGapLeft + queueGap * index);
    }

    private static Orientation FlipOrientation(Orientation orientation)
    {
        if (orientation == Orientation.East)
            return Orientation.South;
        return Orientation.East;
    }
}
