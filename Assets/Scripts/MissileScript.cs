using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileScript : MonoBehaviour
{
    public Rigidbody body;
    [Space]
    public List<Snapshot> snapshots;
    public int frame;

    public Action<MissileScript> onExplode;

    private void FixedUpdate()
    {
        if (++frame >= snapshots.Count)
        {
            enabled = false;
            DestroySelfNicely();
            return;
        }

        body.velocity = snapshots[frame].velocity;
        body.MoveRotation(Quaternion.LookRotation(snapshots[frame].velocity));
    }

    private void DestroySelfNicely()
    {
        float maxLifeTime = 0;
        foreach (ParticleSystem child in GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.EmissionModule emission = child.emission;
            emission.enabled = false;
            maxLifeTime = Mathf.Max(maxLifeTime, child.main.startLifetime.constantMax);
        }

        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        Destroy(gameObject, maxLifeTime);

        onExplode?.Invoke(this);
    }

    /// <summary>
    /// Calculates the path for a missile trajectory, starting at <paramref name="position"/>. Assumes a starting normal of (0, 1, 0).
    /// </summary>
    /// <param name="position">Starting position for the missile in world space.</param>
    /// <param name="target">Target position for the missile in world space.</param>
    /// <param name="acceleration">Acceleration, units/second/second.</param>
    /// <param name="topSpeed">Top speed for the projectile, units/second.</param>
    /// <param name="turnSpeed">Top speed for changing direction, degrees/second.</param>
    /// <param name="turnDelay">Time in seconds before missile starts turning.</param>
    /// <param name="timeStep">Time in seconds between each frame.</param>
    /// <param name="maxTime">The maximum travel time in seconds if calculation doesn't reach its target.</param>
    /// <returns>Frame by frame snapshot of the location of the missile.</returns>
    public static List<Snapshot> CalculatePath(Vector3 position, Vector3 target,
        float acceleration, float topSpeed, float turnSpeed, float turnDelay,
        float timeStep, float maxTime = 15)
    {
        Vector3 normal = Vector3.up;
        float speed = 0;
        float time = 0;

        var snapshots = new List<Snapshot>
        {
            new Snapshot(position, normal)
        };

        float accelerationPerTimeStep = acceleration * timeStep;
        float turnSpeedRadiansPerTimeStep = turnSpeed * Mathf.Deg2Rad * timeStep;
        while (time < turnDelay || time < maxTime)
        {
            speed = Mathf.Min(topSpeed, speed + accelerationPerTimeStep);
            Vector3 delta = target - position;

            if (time >= turnDelay)
                normal = Vector3.RotateTowards(normal, delta, turnSpeedRadiansPerTimeStep, 0f);

            Vector3 velocity = normal * speed;
            position += velocity * timeStep;

            if (delta.magnitude >= speed * timeStep)
            {
                snapshots.Add(new Snapshot(position, velocity));
            }
            else
            {
                snapshots.Add(new Snapshot(target, velocity));
                break;
            }

            time += timeStep;
        }

        return snapshots;
    }
}

[Serializable]
public struct Snapshot
{
    public Vector3 position;
    public Vector3 velocity;

    public Snapshot(Vector3 position, Vector3 velocity)
    {
        this.position = position;
        this.velocity = velocity;
    }
}
