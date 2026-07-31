using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BiteZoneController : MonoBehaviour
{
    public string sliceTag = "FoodPiece"; // 삭제할 조각 Tag

    private Collider2D capsuleCollider;

    private void Awake()
    {
        capsuleCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Minigame_2_15.Instance != null)
            {
                Debug.Log($"[클릭 시각] {Minigame_2_15.Instance.ElapsedTime:F3}초");
            }

            // 이 클릭을 리듬 판정 매니저에도 전달 (같은 클릭이 타이밍 판정까지 함께 수행)
            // ReceivePlayerInput -> OnPlayerJudged 이벤트가 동기적으로 발생한다고 가정,
            // 따라서 이 호출 직후 LastJudgement에 "이번 클릭"의 판정 결과가 들어있음
            Minigame_2_15.Instance?.OnPlayerInput();

            bool timingOk = Minigame_2_15.Instance != null
                && Minigame_2_15.Instance.LastJudgement != MiniGameBase.JudgementResult.Miss;

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
                    // Tracker 호출
                    FoodPiecesTracker tracker = col.GetComponentInParent<FoodPiecesTracker>();
                    Debug.Log(tracker);

                    if (timingOk && tracker != null && tracker.PieceEaten(col.gameObject))
                    {
                        // 타겟 조각이면서 타이밍도 맞았을 때만 먹힘 -> tracker가 파괴까지 처리함
                        Debug.Log($"{col.name} 삭제됨 (타겟 + 타이밍 성공)");
                    }
                    else
                    {
                        Debug.Log($"{col.name} 무시됨 (타겟 아니거나 타이밍 Miss)");
                    }
                }
            }
        }
    }
}