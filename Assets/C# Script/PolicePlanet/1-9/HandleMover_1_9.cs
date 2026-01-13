using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleMover_1_9 : MonoBehaviour
{
    //private Vector3 initialPosition;

    //[SerializeField] private Vector3 stretchOffset = new Vector3(0, 2f, 0);
    //[SerializeField] private float stretchDuration = 0.2f;
    //[SerializeField] private float shrinkDuration = 0.2f;

    //void Awake()
    //{
    //    initialPosition = transform.localPosition; // 부모(아마 9-minigame)에 상대 위치 저장
    //}

    //public void PlayStretch()
    //{
    //    Vector3 stretchedPos = initialPosition + stretchOffset;

    //    transform.DOLocalMove(stretchedPos, stretchDuration).OnComplete(() =>
    //    {
    //        transform.DOLocalMove(initialPosition, shrinkDuration);
    //    });
    //}

    private Vector3 initialLocalPosition;

    [SerializeField] private Vector3 stretchOffset = new Vector3(0, 2f, 0);
    [SerializeField] private float stretchDuration = 0.15f;
    [SerializeField] private float shrinkDuration = 0.15f;

    void Awake()
    {
        initialLocalPosition = transform.localPosition;
    }

    public void PlayStretch()
    {
        transform.DOLocalMove(initialLocalPosition + stretchOffset, stretchDuration)
            .OnComplete(() =>
            {
                transform.DOLocalMove(initialLocalPosition, shrinkDuration);
            });
    }
}
