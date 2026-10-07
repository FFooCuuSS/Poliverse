using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BiteZoneController : MonoBehaviour, IPracticeDemoInput
{

    [Header("효과음")]
    [SerializeField] private AudioClip eatingSound;

    public string sliceTag = "FoodPiece"; // 삭제할 조각 Tag

    private Collider2D capsuleCollider;

    // 연습 시범 모드: 마우스 입력 대신, 타겟 조각이 입 영역에 들어오면 자동으로 먹는다.
    private bool isDemoMode;
    private readonly List<Collider2D> demoResults = new List<Collider2D>();

    [Header("시범 모드")]
    [Tooltip("타겟 조각이 입 영역에 겹친 뒤 몇 초 있다가 먹을지")]
    [SerializeField] private float demoBiteDelay = 0.3f;

    // 시범 모드에서 조각마다 입 영역에 처음 겹친 시각을 기록
    private readonly Dictionary<Collider2D, float> demoEnterTime = new Dictionary<Collider2D, float>();
    private readonly List<Collider2D> demoStaleKeys = new List<Collider2D>();

    // 이 컴포넌트에 직접 SetDemoMode가 오거나, Minigame_2_15가 데모 모드면 시범으로 동작
    private bool IsDemoActive =>
        isDemoMode || (Minigame_2_15.Instance != null && Minigame_2_15.Instance.IsDemoMode);

    // IPracticeDemoInput 구현
    public void SetDemoMode(bool on)
    {
        isDemoMode = on;
        demoEnterTime.Clear();
    }

    private void Awake()
    {
        capsuleCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (IsDemoActive)
        {
            DemoAutoBite();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();

            List<Collider2D> results = new List<Collider2D>();
            int count = capsuleCollider.OverlapCollider(filter, results);
            Debug.Log($"겹친 콜라이더 수: {count}");

            for (int i = 0; i < count; i++)
            {
                Collider2D col = results[i];
                Debug.Log($"겹친 오브젝트: {col.name}, Tag: {col.tag}");

                if (col.CompareTag(sliceTag))
                {
                    FoodPiecesTracker tracker = col.GetComponentInParent<FoodPiecesTracker>();

                    // 리듬 타이밍과 무관하게, 이 조각이 "타겟으로 표시된 조각"인지만으로 판정한다.
                    bool isTarget = tracker != null && tracker.targetSlices.Contains(col.gameObject);

                    if (isTarget && tracker.PieceEaten(col.gameObject))
                    {
                        // 타겟 조각을 먹음 -> Perfect (tracker가 파괴까지 처리함)
                        Minigame_2_15.Instance?.OnJudgement(MiniGameBase.JudgementResult.Perfect);
                        PlayEatingSound();
                        Debug.Log($"{col.name} 삭제됨 (타겟 조각 - Perfect)");
                    }
                    else
                    {
                        // 타겟이 아닌 조각을 먹으려 함 -> Miss (조각은 삭제하지 않음)
                        Minigame_2_15.Instance?.OnJudgement(MiniGameBase.JudgementResult.Miss);
                        Debug.Log($"{col.name} 무시됨 (타겟 아님 - Miss)");
                    }
                }
            }
        }
    }

    // 시범 모드: 입 영역에 겹친 조각 중 "타겟 조각"만 골라 먹는다. 타겟이 아닌 조각은 건드리지 않음(Miss 없음).
    // 영역에 닿자마자 먹지 않고, 겹친 상태로 demoBiteDelay초가 지나면 먹는다.
    private void DemoAutoBite()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        int count = capsuleCollider.OverlapCollider(filter, demoResults);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = demoResults[i];
            if (col == null || !col.CompareTag(sliceTag)) continue;

            FoodPiecesTracker tracker = col.GetComponentInParent<FoodPiecesTracker>();
            bool isTarget = tracker != null && tracker.targetSlices.Contains(col.gameObject);
            if (!isTarget) continue;

            // 처음 겹친 조각이면 시각만 기록하고 대기
            if (!demoEnterTime.TryGetValue(col, out float enterTime))
            {
                demoEnterTime[col] = Time.time;
                continue;
            }

            // 아직 대기 시간이 안 지났으면 계속 대기
            if (Time.time - enterTime < demoBiteDelay) continue;

            demoEnterTime.Remove(col);

            if (tracker.PieceEaten(col.gameObject))
            {
                Minigame_2_15.Instance?.OnJudgement(MiniGameBase.JudgementResult.Perfect);
                PlayEatingSound();
                Debug.Log($"{col.name} 삭제됨 (시범 자동 먹기 - Perfect)");
            }
        }

        // 영역을 벗어났거나 파괴된 조각의 기록 정리
        demoStaleKeys.Clear();
        foreach (var key in demoEnterTime.Keys)
        {
            // OverlapCollider(List)는 리스트를 결과 개수만큼만 채우므로 demoResults 전체가 현재 겹친 콜라이더
            if (key == null || !demoResults.Contains(key))
                demoStaleKeys.Add(key);
        }
        foreach (var key in demoStaleKeys)
            demoEnterTime.Remove(key);
    }

    private void PlayEatingSound()
    {
        // GameRoot/Audio가 아직 없을 때(연습 씬 등) NullReferenceException 방지
        if (eatingSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(eatingSound);
        }
    }
}