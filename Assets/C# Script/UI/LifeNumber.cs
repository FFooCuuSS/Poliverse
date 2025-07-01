using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LifeNumber : MonoBehaviour
{
    [SerializeField] private List<GameObject> lives;

    private int currentLives;

    private void Start()
    {
        currentLives = lives.Count;
        UpdateLivesUI();

        foreach (GameObject life in lives)
        {
            PlayBounceAnimation(life.transform);
        }
    }

    public void LoseLife()
    {
        if (currentLives <= 0)
        {
            return;
        }

        currentLives--;
        UpdateLivesUI();
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < lives.Count; i++)
        {
            lives[i].SetActive(i < currentLives);
        }
    }

    private void PlayBounceAnimation(Transform target)
    {
        Sequence seq = DOTween.Sequence();

        float scaleX = 1.05f;
        float scaleY = 0.95f;
        float moveY = -0.05f;
        float duration = 0.15f;

        Vector3 originalScale = target.localScale;
        Vector3 squashedScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
        Vector3 stretchedScale = new Vector3(originalScale.x * 0.95f, originalScale.y * 1.05f, originalScale.z);
        Vector3 originalPosition = target.localPosition;

        seq.Append(target.DOScale(squashedScale, duration).SetEase(Ease.OutQuad));
        seq.Join(target.DOLocalMoveY(originalPosition.y + moveY, duration).SetEase(Ease.OutQuad));

        seq.Append(target.DOScale(stretchedScale, duration).SetEase(Ease.OutQuad));
        seq.Join(target.DOLocalMoveY(originalPosition.y - moveY, duration).SetEase(Ease.OutQuad));

        seq.Append(target.DOScale(originalScale, duration).SetEase(Ease.OutQuad));
        seq.Join(target.DOLocalMoveY(originalPosition.y, duration).SetEase(Ease.OutQuad));

        seq.SetLoops(-1); // 무한 반복
    }
}
