using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipProtocol.Game;
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

    private void Start()
    {
        queueGapLeft = queueGap;
        queueList = new List<GameShip>(board.ships);
    }

    private void Update()
    {
        SlideTheQueue();

    }

    private void HandleSelectedBoat()
    {
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
}
