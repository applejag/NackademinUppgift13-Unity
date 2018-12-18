using UnityEngine;

namespace Extensions
{
    public static class CameraExtensions
    {
        private static Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        public static Vector3 ScreenToFlatWorldPoint(this Camera camera, Vector3 mousePosition)
        {
            Ray ray = camera.ScreenPointToRay(mousePosition);

            return groundPlane.Raycast(ray, out float distance)
                ? ray.GetPoint(distance)
                : Vector3.negativeInfinity;
        }
    }
}
