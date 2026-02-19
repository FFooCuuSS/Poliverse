using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlayerHold : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float stayDuration = 0.5f;   //아래에 머무는 시간
    [SerializeField] private float holdTargetY = -3.5f;

    private float startY;
    private bool isMoving;

    private Minigame_2_1 minigame_2_1;

    private void Awake()
    {
        minigame_2_1 = GetComponentInParent<Minigame_2_1>();
        startY = transform.position.y;
    }

    void Update()
    {
        if (isMoving) return;
        if (minigame_2_1 != null && minigame_2_1.IsInputLocked) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
            StartCoroutine(DownAndUp());
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            StartCoroutine(DownAndUp());
#endif
    }

    private IEnumerator DownAndUp()
    {
        isMoving = true;

        if (minigame_2_1 != null)
            minigame_2_1.OnPlayerInput("Hold");

        transform.DOKill();

        // 내려가기
        yield return transform.DOMoveY(holdTargetY, moveDuration)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();

        // 유지
        yield return new WaitForSeconds(stayDuration);

        // 다시 올라오기
        yield return transform.DOMoveY(startY, moveDuration)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();

        isMoving = false;
    }
}
