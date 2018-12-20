using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipProtocol.Game;
using Extensions;
using UnityEngine;
using UnityEngine.Events;

public class GameBoard : MonoBehaviour
{
    public Transform boardToWorldTransform;

    public List<GameShip> ships;
    public Board protocolBoard = new Board();

    [Space]
    public GameObject[] visuals;

    [SerializeField, HideInInspector]
    private Camera cam;

    [SerializeField, HideInInspector]
    private bool isRevealed;

    private void Awake()
    {
        cam = Camera.main;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!boardToWorldTransform) return;

        bool selected = UnityEditor.Selection.transforms.Any(t => t.IsChildOf(transform));

        Color green = Color.green;
        if (!selected)
            green.a = 0.3f;

        Gizmos.color = green;
        Gizmos.DrawWireCube(boardToWorldTransform.TransformPoint(5, 0, 5), boardToWorldTransform.TransformVector(10, 0, 10));

        if (selected)
        {
            UnityEditor.Handles.color = green;
            UnityEditor.Handles.Label(boardToWorldTransform.TransformPoint(0.5f, 0, 0.5f), "A1");
            UnityEditor.Handles.Label(boardToWorldTransform.TransformPoint(0.5f, 0, 9.5f), "J1");
            UnityEditor.Handles.Label(boardToWorldTransform.TransformPoint(9.5f, 0, 0.5f), "A10");
            UnityEditor.Handles.Label(boardToWorldTransform.TransformPoint(9.5f, 0, 9.5f), "J10");
        }
    }
#endif

    private void Start()
    {
        isRevealed = true;
        SetShown(false);
    }

    public void SetShown(bool shown)
    {
        if (isRevealed == shown)
            return;

        isRevealed = shown;
        foreach (GameObject visual in visuals)
        {
            visual.SetActive(shown);
        }
    }

    public Vector2Int WorldToCoordinate(Vector3 pos)
    {
        Vector3 inverseTransformPoint = boardToWorldTransform.InverseTransformPoint(pos);
        return Vector2Int.FloorToInt(new Vector2(inverseTransformPoint.x, inverseTransformPoint.z));
    }

    public Vector3 CoordinateToWorld(Vector2Int pos, float y = 0)
    {
        Vector3 point = boardToWorldTransform.TransformPoint(new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
        point.y = y;
        return point;
    }

    public Vector2Int ScreenSpaceToCoordinate(Vector3 pos)
    {
        return WorldToCoordinate(cam.ScreenToFlatWorldPoint(pos));
    }

    public Vector3 OrientationToDirection(Orientation orientation)
    {
        if (orientation == Orientation.South)
            return boardToWorldTransform.TransformDirection(Vector3.forward);

        return boardToWorldTransform.TransformDirection(Vector3.right);
    }

    public Quaternion OrientationToRotation(Orientation orientation)
    {
        return Quaternion.LookRotation(OrientationToDirection(orientation), transform.up);
    }
}
