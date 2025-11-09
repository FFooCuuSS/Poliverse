using UnityEngine;

public class FoodPiecesTracker : MonoBehaviour
{
    public int totalPieces; // 시작할 때 조각 개수

    private int eatenPieces = 0;

    // 한 조각이 먹혔을 때 호출
    public void PieceEaten()
    {
        eatenPieces++;
        Debug.Log($"조각 먹힘: {eatenPieces}/{totalPieces}");

        if (eatenPieces >= totalPieces)
        {
            Debug.Log("모든 조각 먹음! 다음 음식 스폰!");
            // 다음 음식 스폰 호출 (FoodManager 같은 곳)
            FoodManager.Instance.SpawnNextFood();
            Destroy(gameObject);

        }
    }
}
