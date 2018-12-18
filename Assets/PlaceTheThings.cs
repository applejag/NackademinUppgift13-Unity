using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlaceTheThings : MonoBehaviour
{
    public RectTransform prefab;
    public bool run;

    void Update()
    {
        if (!run) return;
        run = false;

        for (var y = 9; y >= 0; y--)
        {
            for (var x = 0; x < 10; x++)
            {
                GameObject clone = Instantiate(prefab.gameObject);
                var rect = clone.GetComponent<RectTransform>();

                clone.transform.parent = transform;
                clone.name = $"({x},{9-y})";

                rect.anchorMin = new Vector2(x / 10f, y / 10f);
                rect.anchorMax = new Vector2((x + 1) / 10f, (y + 1) / 10f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }
}
