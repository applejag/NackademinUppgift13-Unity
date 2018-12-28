using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipProtocol.Game;
using UnityEngine;

public class GameShip : MonoBehaviour
{
    public GameBoard board;
    public ShipType shipType;
    public Vector3 offset;
    public Vector3 missileSilo;

    [Space]
    public Transform model;
    public float modelRotationYEast;
    public float modelRotationYSouth;


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetMissileSiloWorldPosition(), 0.5f);
        Gizmos.DrawRay(GetMissileSiloWorldPosition() + Vector3.up * 0.5f, Vector3.up * 5);
    }

    private void OnDrawGizmos()
    {
        if (!board) return;
        
        Color yellow = Color.yellow;
        Color green = Color.green;
        if (UnityEditor.Selection.transforms.Length == 0 || UnityEditor.Selection.transforms.All(t => !transform.IsChildOf(t)))
        {
            yellow.a = 0.2f;
            green.a = 0.2f;
        }

        Vector3 world = board.CoordinateToWorld(GetBoardCoordinateFromPosition());
        Gizmos.color = yellow;
        Gizmos.DrawLine(world, transform.position);
        Gizmos.color = green;
        Vector3 size = board.boardToWorldTransform.TransformVector(1, 0.5f, 1);
        Gizmos.DrawWireCube(world + Vector3.up * size.y * 0.5f, size);
    }
#endif

    public Vector3 GetMissileSiloWorldPosition()
    {
        return model.TransformPoint(missileSilo);
    }

    public void SetPositionFromCoordinate(Vector2Int coordinate, Orientation orientation, float y = 0f)
    {
        Vector3 world = board.CoordinateToWorld(coordinate, y);

        transform.rotation = board.OrientationToRotation(orientation);
        transform.position = world + GetPositionOffset();

        if (model)
        {
            Vector3 eulerAngles = model.transform.localEulerAngles;
            eulerAngles.y = orientation == Orientation.East ? modelRotationYEast : modelRotationYSouth;
            model.transform.localEulerAngles = eulerAngles;
        }
    }

    public Vector2Int GetBoardCoordinateFromPosition()
    {
        return board.WorldToCoordinate(transform.position - GetPositionOffset());
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
