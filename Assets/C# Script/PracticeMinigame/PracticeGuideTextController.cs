using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PracticeGuideTextController :
    MonoBehaviour
{
    [SerializeField]
    private TMP_Text guideText;

    private readonly List<string> guideTexts =
        new List<string>();

    private int unlockedCount;
    private int currentIndex;

    public void Initialize(
        IReadOnlyList<string> texts)
    {
        guideTexts.Clear();

        if (texts != null)
        {
            for (int i = 0;
                 i < texts.Count;
                 i++)
            {
                if (!string.IsNullOrWhiteSpace(
                        texts[i]))
                {
                    guideTexts.Add(
                        texts[i]
                    );
                }
            }
        }

        if (guideTexts.Count == 0)
        {
            unlockedCount = 0;
            currentIndex = 0;

            Refresh();
            return;
        }

        // 첫 설명은 처음부터 해금.
        unlockedCount = 1;
        currentIndex = 0;

        Refresh();
    }

    public void UnlockGuide(int index)
    {
        if (guideTexts.Count == 0)
            return;

        if (index < 0 ||
            index >= guideTexts.Count)
        {
            Debug.LogWarning(
                $"[PracticeGuide] " +
                $"잘못된 설명 인덱스: {index}"
            );

            return;
        }

        /*
         * 2번 설명을 해금했다면
         * 0~2번까지 사용 가능하다고 본다.
         */
        unlockedCount =
            Mathf.Max(
                unlockedCount,
                index + 1
            );

        currentIndex = index;

        Refresh();
    }

    public void ShowNextGuide()
    {
        if (unlockedCount <= 1)
            return;

        currentIndex++;

        if (currentIndex >=
            unlockedCount)
        {
            currentIndex = 0;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (guideText == null)
            return;

        if (guideTexts.Count == 0 ||
            unlockedCount == 0)
        {
            guideText.text = "";
            return;
        }

        guideText.text =
            guideTexts[currentIndex];
    }
}