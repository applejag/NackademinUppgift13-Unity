using BattleshipProtocol.Game;
using UnityEngine;

public class TmpMousePointVisual : MonoBehaviour
{
    public GameObject quad;

    private Plane plane = new Plane(Vector3.up, Vector3.zero);
    private new Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        if (camera == null)
            return;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float dist))
        {
            Vector3Int pos = GetPosition(ray.GetPoint(dist));

            if (Board.IsOnBoard(pos.x, pos.z))
            {
                quad.transform.position = new Vector3(pos.x + 0.5f, 0, pos.z + 0.5f);

                if (!quad.activeSelf)
                    quad.SetActive(true);

                return;
            }
        }

        if (quad.activeSelf)
        {
            quad.SetActive(false);
        }
    }

    private static Vector3Int GetPosition(Vector3 pos)
    {
        return Vector3Int.FloorToInt(new Vector3(pos.x, 0, pos.z));
    }
}
