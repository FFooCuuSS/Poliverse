using System.Collections.Generic;
using UnityEngine;

// 3-8 박스 생성. 생성 시각은 Minigame3_8remake가 노래 시간으로 정해서 SpawnBush를 부른다.
public class BushSpawner3_8 : MonoBehaviour
{
    public Minigame3_8remake minigame;
    public GameObject bushPrefab;
    public EnemyWatch3_8 enemyWatch;   // (구버전 호환, 사용 안 함)
    public int bushCount = 0;

    public Vector3 spawnPosition = new Vector3(10f, 0f, 0f);

    public float spawnInterval = 4.8f; // (구버전 호환, 사용 안 함)

    private readonly List<BushMover3_8> bushes = new List<BushMover3_8>();

    private void Awake()
    {
        if (minigame == null)
            minigame = GetComponentInParent<Minigame3_8remake>();
    }

    void Start()
    {
        bushCount = 0;
    }

    void Update()
    {
        Cleanup();
    }

    // 타이밍 맞게 숨었을 때 Minigame3_8remake가 호출 (박스가 잠깐 멈춤)
    public void SimulateDemoClick()
    {
        StopAllBushes();
    }

    // lateBy: 원래 생성 시각보다 늦은 만큼 (프레임 지연 보정)
    public void SpawnBush(float lateBy = 0f)
    {
        if (bushPrefab == null) return;

        bushCount++;

        GameObject obj = Instantiate(bushPrefab, spawnPosition, Quaternion.identity, transform);

        BushMover3_8 mover = obj.GetComponent<BushMover3_8>();
        if (mover != null)
        {
            if (lateBy > 0f)
                obj.transform.position += Vector3.left * mover.moveSpeed * lateBy;

            bushes.Add(mover);
        }
    }

    void StopAllBushes()
    {
        for (int i = 0; i < bushes.Count; i++)
        {
            if (bushes[i] != null)
                bushes[i].StopForMoment();
        }
    }

    void Cleanup()
    {
        for (int i = bushes.Count - 1; i >= 0; i--)
        {
            if (bushes[i] == null)
                bushes.RemoveAt(i);
        }
    }
}