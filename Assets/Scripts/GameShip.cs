using System;
using System.Collections;
using System.Collections.Generic;
using BattleshipProtocol.Game;
using UnityEngine;

public class GameShip : MonoBehaviour
{
    public GameBoard board;
    public ShipType shipType;
    public Vector3 offset;

    public void SetPositionFromCoordinate(Vector2Int coordinate, Orientation orientation, float y = 0f)
    {
        Vector3 world = board.CoordinateToWorld(coordinate, y);

        transform.rotation = board.OrientationToRotation(orientation);
        transform.position = world + GetPositionOffset();
    }

    public Vector3 GetPositionOffset()
    {
        return transform.TransformVector(offset);
    }

    public Ship GetShip()
    {
        return board == null 
            ? null : board.protocolBoard.GetShip(shipType);
    }

    public int GetLength()
    {
        if (board == null) return -1;
        if (!Enum.IsDefined(typeof(ShipType), shipType)) return -1;
        return Ship.GetShipLength(shipType);
    }
}
