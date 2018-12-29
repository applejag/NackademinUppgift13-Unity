using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipProtocol.Game;
using UnityEngine;
using Random = UnityEngine.Random;

public class MissileSiloScript : MonoBehaviour
{
    public GameObject missilePrefab;
    public GameShip[] fireFrom;
    [Space]
    public GameObject explosionHitPrefab;
    public GameObject explosionMissPrefab;

    [Header("Settings")]
    public float topSpeed = 10;
    public float acceleration = 10;
    public float turnSpeed = 270;
    public float turnDelay = 1.5f;

#if UNITY_EDITOR
    [Header("Testing")]
    public Transform target;
    public bool fire;
    [Space]
    public Color startColor = Color.red;
    public Color endColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (target == null) return;
        List<Snapshot> snapshots = CalculateMyPath(transform.position, target.position);

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

    private void Update()
    {
        if (!fire) return;
        fire = false;

        FireMissile(GetFrom(), target.position);
    }
#endif

    private List<Snapshot> CalculateMyPath(Vector3 from, Vector3 to)
    {
        return MissileScript.CalculatePath(from, to, acceleration, topSpeed, turnSpeed, turnDelay,
            Time.fixedDeltaTime);
    }

    private MissileScript FireMissile(Vector3 from, Vector3 to, Action<MissileScript> onExplode = null)
    {
        GameObject clone = Instantiate(missilePrefab, from, Quaternion.identity);
        var missileScript = clone.GetComponent<MissileScript>();
        missileScript.snapshots = CalculateMyPath(from, to);
        missileScript.onExplode = onExplode;

        return missileScript;
    }

    public MissileScript FireMissileHit(Vector3 to, Action<MissileScript> onExplode = null)
    {
        return FireMissileWithExplosion(GetFrom(), to, explosionHitPrefab, onExplode);
    }

    public MissileScript FireMissileMiss(Vector3 to, Action<MissileScript> onExplode = null)
    {
        return FireMissileWithExplosion(GetFrom(), to, explosionMissPrefab, onExplode);
    }

    private MissileScript FireMissileWithExplosion(Vector3 from, Vector3 to, GameObject explosion, Action<MissileScript> onExplode = null)
    {
        return FireMissile(from, to, script =>
        {
            var position = new Vector3(to.x, explosion.transform.position.y, to.z);
            Instantiate(explosion, position, Quaternion.identity);

            onExplode?.Invoke(script);
        });
    }

    private Vector3 GetFrom()
    {
        GameShip[] ships = fireFrom
            .Where(gameShip =>
            {
                Ship s = gameShip.GetShip();
                return s != null && s.IsOnBoard && s.Health > 0;
            })
            .ToArray();

        if (ships.Length > 0)
            return ships[Random.Range(0, ships.Length)].GetMissileSiloWorldPosition();

        return transform.position;
    }

}
