using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCameraRig : MonoBehaviour
{
    public float maxZ = 250;
    public float minZ = 50;

    public float dragSpeed = 2;

    [SerializeField, HideInInspector]
    private new Camera camera;

    [SerializeField, HideInInspector]
    private Vector3 dragRayOrigin;

    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0)) return;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 rayPosition = ray.GetPoint(distance);

        if (Input.GetMouseButtonDown(0))
        {
            dragRayOrigin = rayPosition;
        }

        Vector3 delta = dragRayOrigin - rayPosition;
        Vector3 position = transform.position;
        Vector3 target = position + delta;

        target.z = Mathf.Clamp(target.z, minZ, maxZ);
        target.x = position.x;

        Vector3 diff = target - position;
        transform.position = target;
        
        dragRayOrigin = rayPosition + diff;
    }

}
