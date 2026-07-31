using System.Collections.Generic;
using UnityEngine;

public class FoodPiecesTracker : MonoBehaviour
{
    public int totalPieces; // 전체 조각 개수
    public List<GameObject> allSlices = new List<GameObject>(); // 전체 조각
    public List<GameObject> targetSlices = new List<GameObject>(); // FoodAssembler가 수동 설정으로 채워줌 (먹어야 하는 조각)

    private int eatenTargetCount = 0;

    // BiteZoneController가 특정 조각을 클릭했을 때 호출.
    // 타겟이 아니면 무시하고 false 반환 -> 파괴되지 않음.
    public bool PieceEaten(GameObject piece)
    {
        if (piece == null) return false;
        if (!targetSlices.Contains(piece)) return false; // 타겟이 아니면 먹을 수 없음

        targetSlices.Remove(piece);
        eatenTargetCount++;
        Destroy(piece);

        Debug.Log($"타겟 조각 먹음: {eatenTargetCount}개 완료, 남은 타겟 {targetSlices.Count}개");
        return true;
    }
}