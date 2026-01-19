using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AccessoryBlinkManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float minAlpha = 0.5f;    
    [SerializeField] private float fadeDuration = 0.5f;

    private readonly List<Accessory> accessories = new List<Accessory>();


    public void RegisterAccessory(Accessory acc)
    {
        if (acc == null) return;
        accessories.Add(acc);
        acc.LockInput();

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

        float[] blinkCenters = { 1f, 2f, 3f };

        for (int i = 0; i < accessories.Count && i < blinkCenters.Length; i++)
        {
            Accessory acc = accessories[i];
            if (acc == null) continue;

            float startAt = Mathf.Max(0f, blinkCenters[i] - fadeDuration);

            var sr = acc.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            sr.DOKill();

            Sequence seq = DOTween.Sequence();
            seq.SetDelay(startAt);
            seq.Append(sr.DOFade(minAlpha, fadeDuration));
            seq.Append(sr.DOFade(1f, fadeDuration));
        }

        yield return new WaitForSeconds(3f + fadeDuration);


        var game = FindObjectOfType<Minigame_1_4>();
        game.SetAccessoryOrder(accessories);

        foreach (var acc in accessories)
            acc.UnlockInput();

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
