using System.Collections;
using UnityEngine;

// 카메라 뷰포트를 이용해 화면 바깥 랜덤지점에서 오브젝트를 생성/투척
public class OffscreenThrowSpawner2D : MonoBehaviour
{

    public Camera mainCam;
    public Throwable2D bomb;
    public Throwable2D paper;



    public float minSpawnInterval = 0.6f;
    public float maxSpawnInterval = 1.5f;

    
    public float offscreenMargin = 1.5f;


    // 0~1 뷰포트 좌표 범위 중, 던질 대략의 목표 사각형(중앙으로 모이게 하려면 0.35~0.65 정도 추천)
    private Vector2 targetViewportMin = new Vector2(0.35f, 0.5f);
    private Vector2 targetViewportMax = new Vector2(0.65f, 0.9f);

    [Header("스폰하는 가장자리 선택 비율")]
    // 아래(바닥)에서 많이 나오는 Fruit Ninja 느낌을 주려면 bottomBias를 크게
    [Range(0f, 1f)] private float bottomBias = 0.6f;
    [Range(0f, 1f)] private float leftRightBias = 0.3f; // 좌/우에서 나올 확률 합

    private void Reset()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    public void SpawnOne()
    {
        // 목표 지점
        Vector2 vpTarget = new Vector2(
            Random.Range(targetViewportMin.x, targetViewportMax.x),
            Random.Range(targetViewportMin.y, targetViewportMax.y)
        );
        Vector3 targetWorld = mainCam.ViewportToWorldPoint(new Vector3(vpTarget.x, vpTarget.y, 0f));
        targetWorld.z = 0f;

        // 스폰 가장자리 선택
        Vector3 spawnPos = PickOffscreenPoint();

        Throwable2D prefab = (Random.value < 0.5f) ? bomb : paper;

        Throwable2D obj = Instantiate(prefab);
        obj.ThrowFromTo(spawnPos, targetWorld);
    }

    private Vector3 PickOffscreenPoint()
    {
        // 카메라 밖 경계 설정
        Vector3 bl = mainCam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)); // bottom-left
        Vector3 tr = mainCam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f)); // top-right

        float leftX = bl.x - offscreenMargin;
        float rightX = tr.x + offscreenMargin;
        float bottomY = bl.y - offscreenMargin;
        float topY = tr.y + offscreenMargin;

        // 가중치로 가장자리 선택
        float r = Random.value;
        // 아래
        if (r < bottomBias)
        {
            float x = Random.Range(bl.x, tr.x);
            return new Vector3(x, bottomY, 0f);
        }
        // 좌우
        else if (r < bottomBias + leftRightBias)
        {
            bool left = Random.value < 0.5f;
            float y = Random.Range(bl.y, tr.y);
            return new Vector3(left ? leftX : rightX, y, 0f);
        }
        // 위
        else
        {
            float x = Random.Range(bl.x, tr.x);
            return new Vector3(x, topY, 0f);
        }
    }

    // 에디터에서 대략적인 목표영역 확인용
    private void OnDrawGizmosSelected()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 a = mainCam.ViewportToWorldPoint(new Vector3(targetViewportMin.x, targetViewportMin.y, 0f));
        Vector3 b = mainCam.ViewportToWorldPoint(new Vector3(targetViewportMax.x, targetViewportMin.y, 0f));
        Vector3 c = mainCam.ViewportToWorldPoint(new Vector3(targetViewportMax.x, targetViewportMax.y, 0f));
        Vector3 d = mainCam.ViewportToWorldPoint(new Vector3(targetViewportMin.x, targetViewportMax.y, 0f));
        a.z = b.z = c.z = d.z = 0f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, c); Gizmos.DrawLine(c, d); Gizmos.DrawLine(d, a);
    }
}
