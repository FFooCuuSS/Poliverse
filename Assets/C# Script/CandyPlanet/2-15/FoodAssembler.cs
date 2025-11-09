using System.Collections.Generic;
using UnityEngine;

public class FoodAssembler : MonoBehaviour
{
    public List<GameObject> slices; // 조각 프리팹 리스트

    public void AssembleSlices(Transform parent)
    {
        FoodPiecesTracker tracker = parent.GetComponent<FoodPiecesTracker>();
        tracker.totalPieces = slices.Count; // 조각 개수 설정
        foreach (GameObject slicePrefab in slices)
        {
            GameObject slice = Instantiate(slicePrefab, parent); // ✅ 부모 지정
            slice.transform.localPosition = Vector3.zero; // 중심 맞추기
            slice.transform.localRotation = Quaternion.identity;
        }
    }
}
