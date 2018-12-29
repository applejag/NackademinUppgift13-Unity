using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RevealGrid : MonoBehaviour
{
    public float delayBetweenLines = 0.1f;
    public float delayBetweenEastSouthStart = 0.25f;
    public AnimationCurve showLineCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve hideLineCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public AnimationCurve borderShowCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(2, 1, 1, 0));
    public AnimationCurve borderHideCurve = AnimationCurve.EaseInOut(0, 1, 2, 0);
    [Space]
    public LineRenderer[] eastLines;
    public LineRenderer[] southLines;
    public LineRenderer[] borderLines;

    private void Start()
    {
        foreach (LineRenderer line in eastLines
            .Concat(southLines)
            .Concat(borderLines))
        {
            line.enabled = false;
        }
    }

    public void OnBoardShown()
    {
        StartCoroutine(SlowShowAll(showLineCurve));
        StartCoroutine(SlowShowBorder(borderShowCurve));
    }

    public void OnBoardHidden()
    {
        if (!eastLines.First().enabled)
            return;

        StartCoroutine(SlowShowAll(hideLineCurve));
        StartCoroutine(SlowShowBorder(borderHideCurve));
    }

    private IEnumerator SlowShowAll(AnimationCurve curve)
    {
        StartCoroutine(SlowShowLines(eastLines, curve));
        yield return new WaitForSeconds(delayBetweenEastSouthStart);
        StartCoroutine(SlowShowLines(southLines, curve));
    }

    private IEnumerator SlowShowLines(IEnumerable<LineRenderer> lines, AnimationCurve curve)
    {
        foreach (LineRenderer line in lines)
        {
            StartCoroutine(SlowShowLine(line, curve));
            yield return new WaitForSeconds(delayBetweenLines);
        }
    }

    private IEnumerator SlowShowLine(LineRenderer line, AnimationCurve curve)
    {
        float time = 0;
        float duration = curve.keys[curve.length - 1].time;

        line.enabled = true;
        while (time < duration)
        {
            line.SetPosition(1, new Vector3(0, 0, curve.Evaluate(time)));
            yield return null;
            time += Time.deltaTime;
        }

        float lastLength = curve.Evaluate(duration);
        line.SetPosition(1, new Vector3(0, 0, lastLength));
        if (Mathf.Approximately(lastLength, 0))
            line.enabled = false;
    }

    private IEnumerator SlowShowBorder(AnimationCurve curve)
    {
        float time = 0;
        float duration = curve.keys[curve.length - 1].time;

        foreach (LineRenderer line in borderLines)
        {
            line.enabled = true;
        }

        while (time < duration)
        {
            float value = curve.Evaluate(time);
            for (var i = 0; i < borderLines.Length; i++)
            {
                borderLines[i].SetPosition(1, new Vector3(0, BorderLineCalc(i, value), 0));
            }
            yield return null;
            time += Time.deltaTime;
        }

        float lastLength = curve.Evaluate(duration);
        for (var i = 0; i < borderLines.Length; i++)
        {
            if (Mathf.Approximately(lastLength, 0))
                borderLines[i].enabled = false;
            else
                borderLines[i].SetPosition(1, new Vector3(0, BorderLineCalc(i, lastLength), 0));
        }

    }

    private static float BorderLineCalc(int line, float value)
    {
        return Mathf.Clamp01((value - line / 4f) * 4f);
    }
}
