using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RevealLetters : MonoBehaviour
{
    public float timeBetweenSymbols = 0.15f;
    public float timeBetweenEastSouthStart = 0.25f;
    public Color symbolColor = new Color(1, 0, 0, 0.5882353f);
    public Color symbolRevealColor = new Color(1, 0.8f, 0.8f, 1f);
    public Color symbolHideColor = new Color(1, 0, 0, 0f);
    public AnimationCurve symbolRevealCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve symbolShowCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Space]
    public TextMesh[] letters;
    public TextMesh[] numbers;

    private void Start()
    {
        foreach (TextMesh textMesh in letters
            .Concat(numbers))
        {
            textMesh.gameObject.SetActive(false);
        }
    }

    public void OnBoardShown()
    {
        StartCoroutine(FadeTextAll(symbolRevealColor, symbolColor, symbolRevealCurve));
    }

    public void OnBoardHidden()
    {
        if (!letters.First().gameObject.activeSelf)
            return;

        StartCoroutine(FadeTextAll(symbolColor, symbolHideColor, symbolRevealCurve));
    }

    private IEnumerator FadeTextAll(Color from, Color to, AnimationCurve curve)
    {
        StartCoroutine(FadeTextList(letters, from, to, curve));
        yield return new WaitForSeconds(timeBetweenEastSouthStart);
        StartCoroutine(FadeTextList(numbers, from, to, curve));
    }

    private IEnumerator FadeTextList(IEnumerable<TextMesh> texts, Color from, Color to, AnimationCurve curve)
    {
        foreach (TextMesh textMesh in texts)
        {
            StartCoroutine(FadeTextColor(textMesh, from, to, curve));
            yield return new WaitForSeconds(timeBetweenSymbols);
        }
    }

    private static IEnumerator FadeTextColor(TextMesh text, Color from, Color to, AnimationCurve curve)
    {
        float time = 0;
        float duration = curve.keys[curve.length - 1].time;

        text.gameObject.SetActive(true);
        while (time < duration)
        {
            text.color = Color.Lerp(from, to, curve.Evaluate(time));
            yield return null;
            time += Time.deltaTime;
        }

        Color lastColor = Color.Lerp(from, to, curve.Evaluate(duration));
        text.color = lastColor;
        if (Mathf.Approximately(lastColor.a, 0))
            text.gameObject.SetActive(false);
    }

}
