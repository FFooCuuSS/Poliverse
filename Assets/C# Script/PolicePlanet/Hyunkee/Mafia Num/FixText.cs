using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RectTransform textRect = GetComponent<RectTransform>();
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();

        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
