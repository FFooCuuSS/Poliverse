using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlayerHold : MonoBehaviour
{

    [Header("효과음")]
    [SerializeField] private AudioClip hideSound;

    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float stayDuration = 0.5f;   //아래에 머무는 시간
    [SerializeField] private float holdTargetY = -3.5f;

    private float startY;
    private bool isMoving;

    private Minigame_2_1 minigame_2_1;
    private RhythmManager rhythmManager;

    private void Awake()
    {
        minigame_2_1 = GetComponentInParent<Minigame_2_1>();
        startY = transform.position.y;
    }

    void Update()
    {
        if (isMoving) return;
        // 데모 모드일 때는 플레이어 직접 입력 차단
        if (minigame_2_1 != null)
        {
            if (minigame_2_1.IsDemoMode) return;
            if (minigame_2_1.IsInputLocked) return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
            StartCoroutine(DownAndUp());
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            StartCoroutine(DownAndUp());
#endif
    }

    public void TriggerDemoHold()
    {
        if (isMoving) return;
        StartCoroutine(DownAndUp());
    }

    private IEnumerator DownAndUp()
    {
        isMoving = true;
        if (hideSound != null)
        {
            GameRoot.Instance.Audio.PlaySfx(hideSound);
        }



        if (rhythmManager == null)
            rhythmManager = FindObjectOfType<RhythmManager>();

        double songTime = rhythmManager != null
            ? AudioSettings.dspTime - rhythmManager.DspStartTime
            : -1;
        Debug.Log($"[PlayerHold] 클릭 시작 SongTime: {songTime:F3}s (CSV와 동일한 시간 기준, RhythmManager found: {rhythmManager != null})");

        if (minigame_2_1 != null)
            minigame_2_1.OnPlayerInput("Input");

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