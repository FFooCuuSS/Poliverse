using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AccessoryBlinkManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Rhythm Sync")]
    [Tooltip("켜면 자체 타이머 대신 CSV의 Show 노트 타이밍에 깜빡인다.\n" +
             "1-4 CSV에 Show 노트(예: 1,2,3 / 9,10,11)를 넣어야 힌트가 나온다.")]
    [SerializeField] private bool useRhythmShow = false;

    public bool UseRhythmShow => useRhythmShow;

    private readonly List<Accessory> accessories = new List<Accessory>();

    public void RegisterAccessory(Accessory acc)
    {
        if (acc == null) return;

        accessories.Add(acc);
        acc.LockInput();

        // 기존 방식: 3개 모이면 자체 타이머로 깜빡임
        if (!useRhythmShow && accessories.Count == 3)
            StartCoroutine(BlinkSequence());

        // 전부 항상 보이게 유지
        acc.gameObject.SetActive(true);
        SetAlpha(acc, 1f);
    }

    public void ClearAccessories()
    {
        accessories.Clear();
    }

    /// <summary>
    /// Show 노트 순간에 index번째 악세사리를 깜빡인다.
    /// 노트 시각에 바로 어두워졌다가 밝아지므로 박자 = 깜빡임 시작.
    /// </summary>
    public void BlinkAt(int index)
    {
        if (index < 0 || index >= accessories.Count) return;

        Accessory acc = accessories[index];
        if (acc == null) return;

        var sr = acc.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.DOKill();

        Color c = sr.color;
        c.a = minAlpha;
        sr.color = c;

        sr.DOFade(1f, fadeDuration * 2f).SetEase(Ease.OutQuad);
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
        if (game != null)
            game.SetAccessoryOrder(accessories);

        foreach (var acc in accessories)
            if (acc != null) acc.UnlockInput();
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