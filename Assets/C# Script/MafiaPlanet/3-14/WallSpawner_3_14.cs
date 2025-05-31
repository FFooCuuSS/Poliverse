using UnityEngine;
using DG.Tweening;

public class WallSpawner_3_14 : MonoBehaviour
{
    public GameObject brightWallPrefab;
    public GameObject normalWallPrefab;
    public GameObject lightPrefab;
    public Transform wallParent;

    public GameObject stage_3_14;
    private Minigame_3_14 minigame_3_14;

    public float wallSpacing = 3.0f;

    private float nextSpawnX = -6f;
    private int clickCount = 0;
    private bool isEnd = false;
    private bool isMoving = false;

    void Start()
    {
        minigame_3_14 = stage_3_14.GetComponent<Minigame_3_14>();

        // 1번: 일반 벽 고정
        Instantiate(normalWallPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity, wallParent);
        nextSpawnX += wallSpacing;

        int wallCount = 1; // 1번까지 생성 완료

        while (wallCount < 11) // 2~11번까지 생성
        {
            if (wallCount >= 9)
            {
                // 9~11: 무조건 밝은 벽만 생성
                Vector3 spawnPos = new Vector3(nextSpawnX, 0, 0);
                Instantiate(brightWallPrefab, spawnPos, Quaternion.identity, wallParent);
                GameObject lightInstance = Instantiate(lightPrefab, spawnPos, Quaternion.identity, wallParent);
                LightMove_3_14 lightMove_3_14 = lightInstance.GetComponent<LightMove_3_14>();
                lightMove_3_14.L_minigame_3_14 = minigame_3_14;

                nextSpawnX += wallSpacing;
                wallCount++;
            }
            else
            {
                // 1~3개 밝은 벽 생성
                int brightCount = Random.Range(1, 4);
                for (int i = 0; i < brightCount && wallCount < 11; i++)
                {
                    Vector3 spawnPos = new Vector3(nextSpawnX, 0, 0);
                    Instantiate(brightWallPrefab, spawnPos, Quaternion.identity, wallParent);

                    GameObject lightInstance = Instantiate(lightPrefab, spawnPos, Quaternion.identity, wallParent);
                    LightMove_3_14 lightMove = lightInstance.GetComponent<LightMove_3_14>();
                    lightMove.L_minigame_3_14 = minigame_3_14;

                    nextSpawnX += wallSpacing;
                    wallCount++;
                }

                // 일반 벽 1개 생성
                if (wallCount < 11)
                {
                    Vector3 spawnPos = new Vector3(nextSpawnX, 0, 0);
                    Instantiate(normalWallPrefab, spawnPos, Quaternion.identity, wallParent);

                    nextSpawnX += wallSpacing;
                    wallCount++;
                }
            }
        }

        // 12번: 일반 벽 고정
        Instantiate(normalWallPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity, wallParent);
    }

    public void MoveWallLeft()
    {
        if (minigame_3_14.IsInputLocked) return;
        if (isMoving) return; // 이미 이동 중이면 무시
        if (clickCount >= 11 && !isEnd)
        {
            minigame_3_14.Succeed();
            Debug.Log("끝");
            isEnd = true;
        }
        if (clickCount >= 12) return;

        isMoving = true; // 이동 시작 → 잠금
        clickCount++;

        Vector3 targetPosition = wallParent.position + new Vector3(-3f, 0f, 0f);

        wallParent.DOMove(targetPosition, 0.2f)
            .SetEase(Ease.Linear)
            .OnComplete(() => isMoving = false); // 이동 끝나면 다시 입력 허용
    }
}
