using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Bottle2_4 : MonoBehaviour
{
    public GameObject _Outline;
    public Stack<Color> liquids = new Stack<Color>();
    public int capacity = 4;
    public SpriteRenderer[] liquidLayers;

    void Start() => InitBottle();

    public void InitBottle()
    {
        UpdateVisual(true);
        this.RemoveLiquid();
        _Outline.SetActive(false);
    }

    public void AddLiquid(Color color)
    {
        if (liquids.Count < capacity)
        {
            liquids.Push(color);
            UpdateVisual(true);
        }
    }

    public Color RemoveLiquid()
    {
        if (liquids.Count > 0)
        {
            Color top = liquids.Pop();
            UpdateVisual(true);
            return top;
        }
        return Color.clear;
    }

    public Color PeekLiquid()
    {
        if (liquids.Count == 0) return Color.clear;

        // 투명 제외
        foreach (var l in liquids)
        {
            if (l != Color.clear) return l;
        }
        return Color.clear;
    }

    public void UpdateVisual(bool useTween = true)
    {
        Color[] arr = liquids.ToArray();
        System.Array.Reverse(arr);

        for (int i = 0; i < liquidLayers.Length; i++)
        {
            if (i < arr.Length && arr[i] != Color.clear)
            {
                if (useTween)
                {
                    liquidLayers[i].DOColor(arr[i], 0.3f); // 0.3초 동안 색상 변경
                }
                else
                {
                    liquidLayers[i].color = arr[i];
                }
                liquidLayers[i].enabled = true;
            }
            else
            {
                liquidLayers[i].enabled = false;
            }
        }
    }
    public bool IsEmpty()
    {
        // 투명(Color.clear)인 액체를 제외하고 실제 내용물이 없으면 빈 병으로 처리
        foreach (var l in liquids)
        {
            if (l != Color.clear) return false;
        }
        return true;
    }
    public bool IsSingleColor() 
    {
        if (IsEmpty()) return true;

        // 색상별 개수 세기
        Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
        foreach (var c in liquids)
        {
            if (c == Color.clear) continue;
            if (!colorCount.ContainsKey(c)) colorCount[c] = 0;
            colorCount[c]++;
        }

        // 같은 색상이 3개 이상이면 성공
        foreach (var kv in colorCount)
        {
            if (kv.Value >= 3) return true;
        }

        return false;
    }

    private void OnMouseDown()
    {
        Debug.Log($"{gameObject.name} 클릭됨!");
        BottleClickManager2_4.Instance.OnBottleClicked(this);
        
    }
}
