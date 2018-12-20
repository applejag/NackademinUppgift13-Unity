using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour
{
    public float slideThreshold = 0.5f;
    public float slideSpeed = 5;

    private Coroutine currentSliding;

    public void SlideTowards(Transform target)
    {
        if (currentSliding != null)
            StopCoroutine(currentSliding);
        currentSliding = StartCoroutine(SlidingCoroutine(target.position));
    }

    private IEnumerator SlidingCoroutine(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > slideThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * slideSpeed);
            yield return null;
        }

        transform.position = target;
        currentSliding = null;
    }
}
