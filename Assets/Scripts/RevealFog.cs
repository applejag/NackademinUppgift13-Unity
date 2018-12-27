using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RevealFog : MonoBehaviour
{
    public float timeBetweenSquares = 0.1f;
    public float timeBetweenRows = 0.3f;
    public float fogRemoveTime = 0.2f;
    [Space]
    public GameObject fogRemover;
    public List<GameObject> fogRows;

    private Vector2Int lastFogRemover = new Vector2Int(-1, -1);
    private ParticleSystem lastFog;

    public void OnBoardShown()
    {
        print("Reveal the fog...");
        StopAllCoroutines();
        StartCoroutine(ShowBoardCoroutine());
    }

    public void OnBoardHidden()
    {
        print("Hide the fog...");
        StopAllCoroutines();

        fogRemover.SetActive(false);
        lastFogRemover = new Vector2Int(-1, -1);
        lastFog = null;

        StartCoroutine(HideBoardCoroutine());
    }

    public void StopTheFogAt(Vector2Int coordinate, bool permanently)
    {
        if (lastFogRemover == coordinate) return;
        lastFogRemover = coordinate;

        ParticleSystem particles = GetParticleSystemAt(coordinate);
        particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        if (!permanently && lastFog)
            lastFog.Play(false);

        lastFog = particles;

        StopCoroutine(nameof(RemoveOldFogParticlesAtCoroutine));
        StartCoroutine(RemoveOldFogParticlesAtCoroutine(particles.transform.position));
    }

    private IEnumerator RemoveOldFogParticlesAtCoroutine(Vector3 position)
    {
        fogRemover.SetActive(true);
        fogRemover.transform.position = position;
        yield return new WaitForSeconds(fogRemoveTime);
        fogRemover.SetActive(false);
        lastFogRemover = new Vector2Int(-1, -1);
    }

    private IEnumerator ShowBoardCoroutine()
    {
        foreach (GameObject row in fogRows)
        {
            StartCoroutine(RevealRowCoroutine(GetParticleSystems(row)));
            yield return new WaitForSeconds(timeBetweenRows);
        }
    }

    private IEnumerator RevealRowCoroutine(ParticleSystem[] row)
    {
        foreach (ParticleSystem particles in row.Reverse())
        {
            particles.Play();
            yield return new WaitForSeconds(timeBetweenSquares);
        }
    }

    private IEnumerator HideBoardCoroutine()
    {
        foreach (GameObject row in fogRows)
        {
            StartCoroutine(HideRowCoroutine(GetParticleSystems(row)));
            yield return new WaitForSeconds(timeBetweenRows);
        }
    }

    private IEnumerator HideRowCoroutine(ParticleSystem[] row)
    {
        foreach (ParticleSystem particles in row)
        {
            particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            yield return new WaitForSeconds(timeBetweenSquares);
        }
    }

    private ParticleSystem[] GetParticleSystems(GameObject row)
    {
        return row.transform
            .Cast<Transform>()
            .Select(t => t.GetComponent<ParticleSystem>())
            .ToArray();
    }

    /// <summary>
    /// Assumes the fog rows are in order A-J, and that particle systems in the rows are in order 1-10
    /// </summary>
    private ParticleSystem GetParticleSystemAt(Vector2Int coordinate)
    {
        return fogRows[coordinate.y]
            .transform.GetChild(coordinate.x)
            .GetComponent<ParticleSystem>();
    }
}
