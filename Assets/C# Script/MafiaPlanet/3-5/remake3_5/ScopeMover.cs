using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeMover : MonoBehaviour
{
    [Header("이동할 좌표들 순서대로 입력")]
    public Vector3[] movePoints;

    [Header("한 구간당 걸리는 시간")]
    public float moveTimePerStep = 0.6f;

    [Header("시작할 때 바로 실행할지")]
    public bool playOnStart;

    [Header("마지막 점까지 간 뒤 반복할지")]
    public bool loop = false;

    private bool isMoving = false;
    public CameraFollow3_5 cameraFollow;


    // 외부에서 호출해서 이동 시작할 때 사용
    public void StartMove()
    {
        if (isMoving) return;
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        if (movePoints == null || movePoints.Length == 0)
            yield break;

        isMoving = true;

        do
        {
            for (int i = 0; i < movePoints.Length; i++)
            {
                Vector3 startPos = transform.position;
                Vector3 targetPos = movePoints[i];

                float elapsed = 0f;

                while (elapsed < moveTimePerStep)
                {
                    elapsed += Time.deltaTime;

                    float t = elapsed / moveTimePerStep;
                    if (t > 1f) t = 1f;

                    transform.position = Vector3.Lerp(startPos, targetPos, t);

                    yield return null;
                }

                transform.position = targetPos;

                //  마지막 점 도달 체크
                if (i == movePoints.Length - 1)
                {
                    if (cameraFollow != null)
                    {
                        Debug.Log("Releasing camera follow");
                        cameraFollow.ReleaseCamera();
                    }

                    isMoving = false;
                    yield break; // 루프 완전 종료
                }
            }

        } while (loop);

        isMoving = false;
    }
}
