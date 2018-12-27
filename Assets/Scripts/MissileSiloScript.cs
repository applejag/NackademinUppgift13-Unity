using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileSiloScript : MonoBehaviour
{
    public Transform target;
    [Space]
    public float topSpeed = 10;
    public float acceleration = 10;
    public float turnSpeed = 270;
    public float turnDelay = 1.5f;
    [Space]
    public Color startColor = Color.red;
    public Color endColor = Color.yellow;
    [Space]
    public GameObject missilePrefab;
    public bool fire;

    private void OnDrawGizmos()
    {
        if (target == null) return;
        List<Snapshot> snapshots = CalculateMyPath();

        Gizmos.color = startColor;
        Gizmos.DrawSphere(transform.position, 0.5f);

        for (var i = 0; i < snapshots.Count - 1; i++)
        {
            Gizmos.color = Color.Lerp(startColor, endColor, i / (float)snapshots.Count);
            Gizmos.DrawLine(snapshots[i].position, snapshots[i + 1].position);
        }

        Gizmos.color = endColor;
        Gizmos.DrawSphere(target.position, 0.5f);
    }

    private List<Snapshot> CalculateMyPath()
    {
        return MissileScript.CalculatePath(transform.position, target.position, acceleration, topSpeed, turnSpeed, turnDelay,
            Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (!fire && !Input.GetButtonDown("Fire1")) return;
        fire = false;

        GameObject clone = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        var missileScript = clone.GetComponent<MissileScript>();
        missileScript.snapshots = CalculateMyPath();
    }

}
