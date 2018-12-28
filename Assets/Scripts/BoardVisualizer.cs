using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleshipProtocol.Game;
using UnityEngine;

[DisallowMultipleComponent]
public class BoardVisualizer : MonoBehaviour
{
    public GameBoard board;
    public CameraSlider cameraSlider;
    public GameCameraRig cameraRig;

    [Space]
    public GameObject prefabAim;
    public GameObject prefabMiss;
    public GameObject prefabHit;

    [Space]
    public MissileSiloScript missileSilo;

    [Header("Can be null")]
    public RevealFog fogRemover;

    [Header("Settings")]
    public bool removeFogOnAim = false;
    public CameraFollowMode cameraFollowMode = CameraFollowMode.LookAtMissile;

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
        Vector3 position = board.CoordinateToWorld(coordinate, transform.position.y);

        prefabAim.SetActive(true);
        prefabAim.transform.position = position;

        if (removeFogOnAim)
            RemoveFogAt(coordinate, false);
    }

    public void ResetAim()
    {
        prefabAim.SetActive(false);
    }

    public void PlaceHitAt(Vector2Int coordinate)
    {
        MissileScript missileScript = missileSilo.FireMissileHit(board.CoordinateToWorld(coordinate), missile =>
        {
            PlacePrefabAt(prefabHit, coordinate);
            cameraRig.enabled = true;
            cameraSlider.StopAllCoroutines();
        });
        FollowWithCamera(missileScript);

        ResetAim();
    }

    public void PlaceMissAt(Vector2Int coordinate)
    {
        MissileScript missileScript = missileSilo.FireMissileMiss(board.CoordinateToWorld(coordinate), missile =>
        {
            PlacePrefabAt(prefabMiss, coordinate);
            cameraRig.enabled = true;
            cameraSlider.StopAllCoroutines();
        });
        FollowWithCamera(missileScript);

        ResetAim();
    }

    private void FollowWithCamera(MissileScript missile)
    {
        if (cameraFollowMode != CameraFollowMode.DoNothing)
        {
            cameraRig.enabled = false;
        }

        if (cameraFollowMode == CameraFollowMode.LookAtMissile)
        {
            cameraSlider.FollowUntilItDies(missile.transform);
        }
        else if (cameraFollowMode == CameraFollowMode.LookAtBoard)
        {
            cameraSlider.SlideTowards(board.transform);
        }
    }

    private void PlacePrefabAt(GameObject prefab, Vector2Int coordinate)
    {
        Vector3 position = board.CoordinateToWorld(coordinate, transform.position.y);

        GameObject clone = Instantiate(prefab, position, prefab.transform.rotation, transform);
        clone.SetActive(true);
        placed.Add(clone);

        RemoveFogAt(coordinate, true);
    }

    private void RemoveFogAt(Vector2Int coordinate, bool permanently)
    {
        if (fogRemover != null)
            fogRemover.StopTheFogAt(coordinate, permanently);
    }

    public void OnShipMoved(Ship ship)
    {
        GameShip gameShip = board.ships.First(s => s.shipType == ship.Type);
        gameShip.OnShipMoved(ship);
    }

    public void OnShipDamaged(Ship ship)
    {
        GameShip gameShip = board.ships.First(s => s.shipType == ship.Type);
        gameShip.OnShipDamaged(ship);
    }

    public enum CameraFollowMode
    {
        DoNothing,
        LookAtMissile,
        LookAtBoard
    }
}
