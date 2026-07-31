using System.Collections.Generic;
using UnityEngine;

public class FoodAssembler : MonoBehaviour
{
    [System.Serializable]
    public class SliceEntry
    {
        public GameObject slicePrefab;
        [Tooltip("체크하면 이 조각이 '먹어야 하는' 타겟으로 지정되고, 생성 시 강조 색으로 표시됨")]
        public bool isTarget;
    }

    [Tooltip("조각 프리팹 + 타겟 여부를 여기서 수동으로 설정 (CSV 타이밍과 맞춰서 지정)")]
    public List<SliceEntry> slices;

    [Header("타겟 조각 강조 색상")]
    public Color targetHighlightColor = Color.yellow;

    public void AssembleSlices(Transform parent)
    {
        FoodPiecesTracker tracker = parent.GetComponent<FoodPiecesTracker>();
        tracker.totalPieces = slices.Count; // 조각 개수 설정
        tracker.allSlices.Clear();
        tracker.targetSlices.Clear();

        foreach (SliceEntry entry in slices)
        {
            GameObject slice = Instantiate(entry.slicePrefab, parent); // ✅ 부모 지정
            slice.transform.localPosition = Vector3.zero; // 중심 맞추기
            slice.transform.localRotation = Quaternion.identity;

            SpriteRenderer sr = slice.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10; // 플레이어보다 큰 값으로
                if (entry.isTarget) sr.color = targetHighlightColor; // 생성 시점부터 다른 색으로
            }

            tracker.allSlices.Add(slice);
            if (entry.isTarget) tracker.targetSlices.Add(slice);
        }
    }
}