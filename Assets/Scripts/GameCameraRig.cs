using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameCameraRig : MonoBehaviour
{
    public float maxZ = 250;
    public float minZ = 50;

    public float dragFalloff = 2;

    [SerializeField, HideInInspector]
    private new Camera camera;

    [SerializeField, HideInInspector]
    private float dragRayOrigin;

    private float dragLastSpeed;

    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 position = transform.position;
        Vector3 min = position;
        Vector3 max = position;
        min.z = minZ;
        max.z = maxZ;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(min, max);
        Gizmos.DrawSphere(min, 1f);
        Gizmos.DrawSphere(max, 1f);
    }
#endif

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            DragCamera();
        }
        else
        {
            PostDragCamera();
        }

    }

    private void PostDragCamera()
    {
        Vector3 currentVector = transform.position;
        float target = Mathf.Clamp(currentVector.z + dragLastSpeed * Time.deltaTime, minZ, maxZ);
        transform.position = new Vector3(currentVector.x, currentVector.y, target);

        dragLastSpeed = Mathf.Lerp(dragLastSpeed, 0, Time.deltaTime * dragFalloff);
    }

    private void DragCamera()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (!groundPlane.Raycast(ray, out float distance))
            return;

        float rayPosition = ray.GetPoint(distance).z;

        if (Input.GetMouseButtonDown(0))
        {
            dragRayOrigin = rayPosition;
        }

        float delta = dragRayOrigin - rayPosition;
        Vector3 currentVector = transform.position;
        float current = currentVector.z;
        float target = Mathf.Clamp(current + delta, minZ, maxZ);
        float diff = target - current;

        transform.position = new Vector3(currentVector.x, currentVector.y, target);

        dragRayOrigin = rayPosition + diff;
        dragLastSpeed = diff / Time.deltaTime;
    }
}
