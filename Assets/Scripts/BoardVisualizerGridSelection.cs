using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BattleshipProtocol.Game;
using UnityEngine;
using UnityEngine.Serialization;

public class BoardVisualizerGridSelection : MonoBehaviour
{
    public float selectionSpeed = 5;
    public Color textIdleColor = new Color(1, 0, 0, 0.5882353f);
    public Color textSelectedColor = new Color(1, 0.9f, 0.9f, 1);
    [Space]
    [FormerlySerializedAs("selectionLetters")]
    public LineRenderer[] selectionLetterLines;
    [FormerlySerializedAs("selectionNumbers")]
    public LineRenderer[] selectionNumberLines;

    public TextMesh[] selectionLetterText;
    public TextMesh[] selectionNumberText;

    [SerializeField, HideInInspector]
    private float selectionLineStartAlpha;

    [SerializeField, HideInInspector]
    private float[] selectionLettersAlpha;
    [SerializeField, HideInInspector]
    private bool[] selectionLettersTarget;
    [SerializeField, HideInInspector]
    private float[] selectionNumbersAlpha;
    [SerializeField, HideInInspector]
    private bool[] selectionNumbersTarget;

    private void Start()
    {
        selectionLineStartAlpha = selectionLetterLines
            .First().material.color.a;
        
        selectionLettersAlpha = new float[selectionLetterLines.Length];
        selectionLettersTarget = new bool[selectionLetterLines.Length];
        selectionNumbersAlpha = new float[selectionNumberLines.Length];
        selectionNumbersTarget = new bool[selectionNumberLines.Length];
    }

    private void Update()
    {
        for (var i = 0; i < selectionLetterLines.Length; i++)
        {
            LineRenderer line = selectionLetterLines[i];
            if (!line.gameObject.activeSelf) continue;

            UpdateLineAlpha(line, selectionLetterText[i], ref selectionLettersAlpha[i], selectionLettersTarget[i]);
        }
        for (var i = 0; i < selectionNumberLines.Length; i++)
        {
            LineRenderer line = selectionNumberLines[i];
            if (!line.gameObject.activeSelf) continue;

            UpdateLineAlpha(line, selectionNumberText[i], ref selectionNumbersAlpha[i], selectionNumbersTarget[i]);
        }
    }

    private void UpdateLineAlpha(LineRenderer line, TextMesh text, ref float currentAlpha, bool activateAlpha)
    {
        if (!activateAlpha && Mathf.Approximately(currentAlpha, 0))
        {
            line.gameObject.SetActive(false);
            text.color = textIdleColor;
        }
        else
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, activateAlpha ? 1f : 0f, selectionSpeed * Time.deltaTime);
            SetLineAlpha(line, text, currentAlpha);
        }
    }

    private void SetLineAlpha(LineRenderer line, TextMesh text, float currentAlpha)
    {
        Color materialColor = line.material.color;
        materialColor.a = selectionLineStartAlpha * currentAlpha;
        line.material.color = materialColor;
        text.color = Color.Lerp(textIdleColor, textSelectedColor, currentAlpha);
    }

    public void SetSelection(int xPos, int yPos, int xLength = 1, int yLength = 1)
    {
        for (var x = 0; x < selectionNumbersTarget.Length; x++)
        {
            if (x >= xPos && x < xPos + xLength)
                EnableLine(selectionNumberLines[x], selectionNumberText[x], ref selectionNumbersAlpha[x], ref selectionNumbersTarget[x]);
            else
                selectionNumbersTarget[x] = false;
        }

        for (var y = 0; y < selectionLettersTarget.Length; y++)
        {
            if (y >= yPos && y < yPos + yLength)
                EnableLine(selectionLetterLines[y], selectionLetterText[y], ref selectionLettersAlpha[y], ref selectionLettersTarget[y]);
            else
                selectionLettersTarget[y] = false;
        }
    }

    public void ResetSelection()
    {
        for (var i = 0; i < selectionLettersTarget.Length; i++)
        {
            selectionLettersTarget[i] = false;
        }

        for (var i = 0; i < selectionNumbersTarget.Length; i++)
        {
            selectionNumbersTarget[i] = false;
        }
    }

    [SuppressMessage("ReSharper", "RedundantAssignment")]
    private void EnableLine(LineRenderer line, TextMesh text, ref float currentAlpha, ref bool activateAlpha)
    {
        if (activateAlpha) return;

        currentAlpha = 0;
        activateAlpha = true;
        line.gameObject.SetActive(true);
        SetLineAlpha(line, text, 0);
    }
}
