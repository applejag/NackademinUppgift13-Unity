using BattleshipProtocol.Game;
using UnityEngine;

public class TmpMousePointVisual : MonoBehaviour
{
    public GameObject quad;

    public Color selectedMeshColor = Color.red;
    public Color unselectedMeshColor = new Color(1, 0, 0, 0.5f);

    [SerializeField] private TextMesh lastTextX;
    [SerializeField] private TextMesh lastTextY;
    private Plane plane = new Plane(Vector3.up, Vector3.zero);
    [SerializeField] private new Camera camera;

    [SerializeField] private TextMesh[] textMeshesX;
    [SerializeField] private TextMesh[] textMeshesY;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.TransformPoint(5, 0 , 5), transform.TransformVector(10, 0, 10));
    }

    private void Awake()
    {
        camera = Camera.main;

        textMeshesX = new TextMesh[10];
        for (var x = 0; x < 10; x++)
        {
            textMeshesX[x] = GameObject.Find((x + 1).ToString()).GetComponent<TextMesh>();
            textMeshesX[x].color = unselectedMeshColor;
        }

        const string letters = "ABCDEFGHIJ";
        textMeshesY = new TextMesh[10];
        for (var y = 0; y < 10; y++)
        {
            textMeshesY[y] = GameObject.Find(letters[y].ToString()).GetComponent<TextMesh>();
            textMeshesY[y].color = unselectedMeshColor;
        }
    }

    private void Update()
    {
        if (camera == null)
            return;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float dist))
        {
            Vector2Int pos = GetWorldToBoard(ray.GetPoint(dist));

            if (Board.IsOnBoard(pos.x, pos.y))
            {
                quad.transform.position = GetBoardToWorld(pos, quad.transform.position.y);

                if (!quad.activeSelf)
                    quad.SetActive(true);

                TextMesh textX = textMeshesX[pos.x];
                TextMesh textY = textMeshesY[pos.y];
                
                SetTextMeshAlpha(ref lastTextX, textX);
                SetTextMeshAlpha(ref lastTextY, textY);

                return;
            }
        }

        SetTextMeshAlpha(ref lastTextX, null);
        SetTextMeshAlpha(ref lastTextY, null);

        if (quad.activeSelf)
        {
            quad.SetActive(false);
        }
    }

    private void SetTextMeshAlpha(ref TextMesh oldMesh, TextMesh newMesh)
    {
        if (oldMesh == newMesh)
            return;

        if (oldMesh != null)
        {
            oldMesh.color = unselectedMeshColor;
        }

        if (newMesh != null)
        {
            newMesh.color = selectedMeshColor;
        }

        oldMesh = newMesh;
    }

    private Vector2Int GetWorldToBoard(Vector3 pos)
    {
        Vector3 inverseTransformPoint = transform.InverseTransformPoint(pos);
        return Vector2Int.FloorToInt(new Vector2(inverseTransformPoint.x, inverseTransformPoint.z));
    }

    private Vector3 GetBoardToWorld(Vector2Int pos, float y = 0)
    {
        Vector3 point = transform.TransformPoint(new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));
        point.y = y;
        return point;
    }
}
