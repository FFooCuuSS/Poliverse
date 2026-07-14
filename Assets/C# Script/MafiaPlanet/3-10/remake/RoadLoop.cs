using UnityEngine;

public class RoadLoop : MonoBehaviour
{
    [Header("이동할 도로 오브젝트들")]
    [SerializeField] private Transform[] roadPieces;

    [Header("도로 이동 속도")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("도로 조각 하나의 가로 길이")]
    [SerializeField] private float roadWidth = 10f;

    [Header("이 위치보다 오른쪽으로 나가면 맨 앞으로 이동")]
    [SerializeField] private float resetX = 12f;

    private void Update()
    {
        MoveRoad();
        CheckRoadPosition();
    }

    private void MoveRoad()
    {
        // 모든 도로 조각을 오른쪽으로 이동시킨다.
        foreach (Transform road in roadPieces)
        {
            road.position += Vector3.right * moveSpeed * Time.deltaTime;
        }
    }

    private void CheckRoadPosition()
    {
        foreach (Transform road in roadPieces)
        {
            // 화면 오른쪽 밖으로 나간 도로를 다시 가장 왼쪽으로 보낸다.
            if (road.position.x >= resetX)
            {
                MoveRoadToFront(road);
            }
        }
    }

    private void MoveRoadToFront(Transform roadToMove)
    {
        // 현재 가장 왼쪽에 있는 도로의 X 위치를 찾는다.
        float leftMostX = roadPieces[0].position.x;

        foreach (Transform road in roadPieces)
        {
            if (road.position.x < leftMostX)
            {
                leftMostX = road.position.x;
            }
        }

        // 오른쪽으로 빠져나간 도로를 가장 왼쪽 도로 앞에 배치한다.
        roadToMove.position = new Vector3(
            leftMostX - roadWidth,
            roadToMove.position.y,
            roadToMove.position.z
        );
    }
}