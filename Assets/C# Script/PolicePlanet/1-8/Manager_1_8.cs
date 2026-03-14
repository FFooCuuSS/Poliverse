using System.Collections.Generic;
using UnityEngine;

public class Manager_1_8 : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Minigame_1_8 minigame;
    [SerializeField] private GameObject prisonObj;

    [Header("Prisoner Prefabs (3 types)")]
    [SerializeField] private GameObject[] prisonerPrefabs;

    [Header("Spawn")]
    [SerializeField] private float spawnY = -3.5f;
    [SerializeField] private float spawnXOffsetFromRightEdge = 1.2f;

    [Header("Speed (2 types only)")]
    [SerializeField] private float slowSpeed = 8f;
    [SerializeField] private float fastSpeed = 16f;

    [Header("Debug Counters")]
    [SerializeField] private int spawnCount = 0;
    [SerializeField] private int capturedCount = 0;
    [SerializeField] private int escapedCount = 0;

    private readonly List<Prisoner_1_8> alivePrisoners = new();

    private void Awake()
    {
        PrisonTrigger prisonTrigger = prisonObj.GetComponent<PrisonTrigger>();
        if (prisonTrigger != null)
            prisonTrigger.manager = this;
    }

    public void ResetRoundState()
    {
        spawnCount = 0;
        capturedCount = 0;
        escapedCount = 0;

        ClearAllPrisoners();

        Debug.Log("[Manager_1_8] 상태 초기화 완료");
    }

    public void SpawnNextPrisoner()
    {
        if (prisonerPrefabs == null || prisonerPrefabs.Length == 0)
        {
            Debug.LogWarning("[Manager_1_8] prisonerPrefabs가 비어있음");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[Manager_1_8] Camera.main 없음");
            return;
        }

        float rightEdgeX = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;
        float spawnX = rightEdgeX + spawnXOffsetFromRightEdge;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        int prefabIndex = spawnCount % prisonerPrefabs.Length;
        GameObject obj = Instantiate(prisonerPrefabs[prefabIndex], spawnPos, Quaternion.identity, transform);

        Prisoner_1_8 prisoner = obj.GetComponent<Prisoner_1_8>();
        if (prisoner == null)
        {
            Debug.LogError("[Manager_1_8] Prisoner_1_8 컴포넌트 없음");
            Destroy(obj);
            return;
        }

        // 2개씩 묶어서 속도 배정
        int speedGroup = (spawnCount / 2) % 2;
        float assignedSpeed = (speedGroup == 0) ? slowSpeed : fastSpeed;

        prisoner.Initialize(this, prisonObj, assignedSpeed);
        alivePrisoners.Add(prisoner);

        spawnCount++;
        Debug.Log($"[Manager_1_8] Spawn #{spawnCount} | prefabIndex={prefabIndex} | speed={assignedSpeed}");
    }

    public void NotifyCaptured(Prisoner_1_8 prisoner)
    {
        if (prisoner == null) return;

        capturedCount++;
        alivePrisoners.Remove(prisoner);

        Debug.Log($"[Manager_1_8] 포획 성공 +1 | capturedCount={capturedCount} | escapedCount={escapedCount} | alive={alivePrisoners.Count}");
    }

    public void NotifyEscaped(Prisoner_1_8 prisoner)
    {
        if (prisoner == null) return;

        escapedCount++;
        alivePrisoners.Remove(prisoner);

        Debug.Log($"[Manager_1_8] 탈출 +1 | capturedCount={capturedCount} | escapedCount={escapedCount} | alive={alivePrisoners.Count}");
    }

    public void ClearAllPrisoners()
    {
        for (int i = alivePrisoners.Count - 1; i >= 0; i--)
        {
            if (alivePrisoners[i] != null)
                Destroy(alivePrisoners[i].gameObject);
        }

        alivePrisoners.Clear();
    }
}