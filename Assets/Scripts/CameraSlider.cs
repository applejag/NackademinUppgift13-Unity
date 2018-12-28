using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSlider : MonoBehaviour
{
    public float slideThreshold = 0.5f;
    public float slideSpeed = 5;
    public GameCameraRig cameraRig;

    public void SlideTowards(Transform target)
    {
        StopAllCoroutines();
        StartCoroutine(SlideFollowCoroutine(target));
    }

    private IEnumerator SlideFollowCoroutine(Transform target)
    {
        while (Mathf.Abs(transform.position.z - target.position.z) > slideThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, TransformTarget(target.position), Time.deltaTime * slideSpeed);
            yield return null;
        }

        transform.position = target.position;
    }

    public void SlideTowards(Vector3 target)
    {
        StopAllCoroutines();
        StartCoroutine(SlideTowardsCoroutine(target));
    }

    private IEnumerator SlideTowardsCoroutine(Vector3 target)
    {
        target = TransformTarget(target);
        while (Vector3.Distance(transform.position, target) > slideThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * slideSpeed);
            yield return null;
        }

        transform.position = target;
    }

    public void FollowUntilItDies(Transform target)
    {
        StopAllCoroutines();
        StartCoroutine(FollowCoroutine(target));
    }

    private IEnumerator FollowCoroutine(Transform target)
    {
        while (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, TransformTarget(target.position), Time.deltaTime * slideSpeed);
            yield return null;
        }
    }

    private Vector3 TransformTarget(Vector3 target)
    {
        Vector3 position = cameraRig.transform.position;
        position.z = Mathf.Clamp(target.z, cameraRig.minZ, cameraRig.maxZ);
        return position;
    }
}
