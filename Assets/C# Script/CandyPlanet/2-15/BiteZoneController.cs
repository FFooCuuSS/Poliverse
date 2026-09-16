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
}