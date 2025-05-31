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

        // 1��: �Ϲ� �� ����
        Instantiate(normalWallPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity, wallParent);
        nextSpawnX += wallSpacing;

        int wallCount = 1; // 1������ ���� �Ϸ�

        while (wallCount < 11) // 2~11������ ����
        {
            if (wallCount >= 9)
            {
                // 9~11: ������ ���� ���� ����
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
                // 1~3�� ���� �� ����
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

                // �Ϲ� �� 1�� ����
                if (wallCount < 11)
                {
                    Vector3 spawnPos = new Vector3(nextSpawnX, 0, 0);
                    Instantiate(normalWallPrefab, spawnPos, Quaternion.identity, wallParent);

                    nextSpawnX += wallSpacing;
                    wallCount++;
                }
            }
        }

        // 12��: �Ϲ� �� ����
        Instantiate(normalWallPrefab, new Vector3(nextSpawnX, 0, 0), Quaternion.identity, wallParent);
    }

    public void MoveWallLeft()
    {
        if (minigame_3_14.IsInputLocked) return;
        if (isMoving) return; // �̹� �̵� ���̸� ����
        if (clickCount >= 11 && !isEnd)
        {
            minigame_3_14.Succeed();
            Debug.Log("��");
            isEnd = true;
        }
        if (clickCount >= 12) return;

        isMoving = true; // �̵� ���� �� ���
        clickCount++;

        Vector3 targetPosition = wallParent.position + new Vector3(-3f, 0f, 0f);

        wallParent.DOMove(targetPosition, 0.2f)
            .SetEase(Ease.Linear)
            .OnComplete(() => isMoving = false); // �̵� ������ �ٽ� �Է� ���
    }
}
