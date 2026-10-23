using UnityEngine;

[ExecuteAlways]
public class BgCoverWidth : MonoBehaviour
{
    public float baseWidth = 1920f;

    void Update()
    {
        var canvasRt = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
        float s = Mathf.Max(1f, canvasRt.rect.width / baseWidth);
        transform.localScale = Vector3.one * s;
       // Debug.Log($"canvas width: {canvasRt.rect.width}, scale: {s}, actual: {transform.localScale}");
    }
}