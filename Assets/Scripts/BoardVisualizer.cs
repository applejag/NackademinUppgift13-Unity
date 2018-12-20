using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardVisualizer : MonoBehaviour
{
    public GameBoard board;

    public GameObject prefabAim;
    public GameObject prefabMiss;
    public GameObject prefabHit;

    [SerializeField, HideInInspector]
    private List<GameObject> placed = new List<GameObject>();

    private void OnEnable()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        foreach (GameObject go in placed)
        {
            Destroy(go);
        }
        placed.Clear();
        ResetAim();
    }

    public void MoveAim(Vector2Int coordinate)
    {
        prefabAim.SetActive(true);
        prefabAim.transform.position = board.CoordinateToWorld(coordinate, transform.position.y);
    }

    public void ResetAim()
    {
        prefabAim.SetActive(false);
    }

    public void PlaceHitAt(Vector2Int coordinate)
    {
        ResetAim();
        PlacePrefabAt(prefabHit, coordinate);
    }

    public void PlaceMissAt(Vector2Int coordinate)
    {
        ResetAim();
        PlacePrefabAt(prefabMiss, coordinate);
    }

    private void PlacePrefabAt(GameObject prefab, Vector2Int coordinate)
    {
        Vector3 position = board.CoordinateToWorld(coordinate, transform.position.y);
        GameObject clone = Instantiate(prefab, position, prefab.transform.rotation, transform);
        clone.SetActive(true);
        placed.Add(clone);
    }
}
