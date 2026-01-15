using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessoryBlinkManager : MonoBehaviour
{
    public float interval = 0.2f;

    private List<Accessory> accessories = new List<Accessory>();

    public void RegisterAccessory(Accessory acc)
    {
        accessories.Add(acc);

        // 전부 항상 보이게 유지
        acc.gameObject.SetActive(true);
        SetAlpha(acc, 1f);
    }

    void Start()
    {
        StartCoroutine(BlinkSequence());
    }

    IEnumerator BlinkSequence()
    {
        yield return new WaitUntil(() => accessories.Count == 3);

        foreach (Accessory acc in accessories)
        {
           
            SetAlpha(acc, 0f);
            yield return new WaitForSeconds(interval);
            SetAlpha(acc, 1f);
            yield return new WaitForSeconds(0.5f);
        }

        
        FindObjectOfType<Minigame_1_4>()
            .SetAccessoryOrder(accessories);
    }

    void SetAlpha(Accessory acc, float alpha)
    {
        var sr = acc.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}
