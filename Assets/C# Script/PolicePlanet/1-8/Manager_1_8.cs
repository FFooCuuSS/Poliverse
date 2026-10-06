using System.Collections.Generic;
using UnityEngine;

public class Manager_1_8 : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Minigame_1_8 minigame;
    [SerializeField] private GameObject prisonObj;

    [Header("Prisoner Prefabs")]
    [SerializeField] private GameObject[] prisonerPrefabs;

    [Header("Spawn")]
    [SerializeField] private float spawnY = -3.5f;
    [SerializeField] private float spawnXOffsetFromRightEdge = 1.2f;

    [Header("Speed")]
    [SerializeField] private float slowSpeed = 8f;
    [SerializeField] private float fastSpeed = 16f;

    [Header("Demo")]
    [SerializeField] private float demoTriggerDistance = 0.1f;

    [Header("Debug Counters")]
    [SerializeField] private int spawnCount = 0;
    [SerializeField] private int capturedCount = 0;
    [SerializeField] private int escapedCount = 0;

    private readonly List<Prisoner_1_8> alivePrisoners = new();

    private PrisonController_1_8 prisonController;

    // 각 죄수가 감옥 작동을 이미 발생시켰는지 저장
    private readonly HashSet<Prisoner_1_8> demoTriggeredPrisoners =
        new HashSet<Prisoner_1_8>();


    private void Awake()
    {
        if (prisonObj == null)
        {
            Debug.LogError(
                "[Manager_1_8] prisonObj가 연결되지 않았습니다."
            );
            return;
        }

        prisonController =
            prisonObj.GetComponent<PrisonController_1_8>();

        if (prisonController == null)
        {
            Debug.LogError(
                "[Manager_1_8] prisonObj에 PrisonController_1_8이 없습니다."
            );
        }

        PrisonTrigger prisonTrigger =
            prisonObj.GetComponent<PrisonTrigger>();

        if (prisonTrigger != null)
        {
            prisonTrigger.manager = this;
        }
    }


    private void Update()
    {
        if (minigame == null)
            return;

        // Player 모드에서는 자동 작동하지 않음
        if (!minigame.IsDemoMode)
            return;

        if (prisonObj == null ||
            prisonController == null)
            return;

        CheckDemoPrisoners();
    }


    private void CheckDemoPrisoners()
    {
        float prisonX =
            prisonObj.transform.position.x;

        for (int i = 0; i < alivePrisoners.Count; i++)
        {
            Prisoner_1_8 prisoner =
                alivePrisoners[i];

            if (prisoner == null)
                continue;

            // 이 죄수는 이미 감옥을 작동시켰으면 무시
            if (demoTriggeredPrisoners.Contains(prisoner))
                continue;

            float prisonerX =
                prisoner.transform.position.x;

            float distance =
                Mathf.Abs(prisonerX - prisonX);

            if (distance <= demoTriggerDistance)
            {
                demoTriggeredPrisoners.Add(prisoner);

                Debug.Log(
                    $"[1-8 Demo] 감옥 작동 : " +
                    $"죄수X={prisonerX:F2}, " +
                    $"감옥X={prisonX:F2}"
                );

                prisonController.ActivatePrison();
            }
        }
    }


    public void ResetRoundState()
    {
        spawnCount = 0;
        capturedCount = 0;
        escapedCount = 0;

        demoTriggeredPrisoners.Clear();

        ClearAllPrisoners();
    }


    public void SpawnNextPrisoner()
    {
        if (prisonerPrefabs == null ||
            prisonerPrefabs.Length == 0)
        {
            Debug.LogWarning(
                "[Manager_1_8] prisonerPrefabs가 비어있습니다."
            );

            return;
        }

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError(
                "[Manager_1_8] Camera.main이 없습니다."
            );

            return;
        }

        float rightEdgeX =
            cam.ViewportToWorldPoint(
                new Vector3(1f, 0f, 0f)
            ).x;

        float spawnX =
            rightEdgeX +
            spawnXOffsetFromRightEdge;

        Vector3 spawnPos =
            new Vector3(
                spawnX,
                spawnY,
                0f
            );

        int prefabIndex =
            spawnCount %
            prisonerPrefabs.Length;

        GameObject obj =
            Instantiate(
                prisonerPrefabs[prefabIndex],
                spawnPos,
                Quaternion.identity,
                transform
            );

        Prisoner_1_8 prisoner =
            obj.GetComponent<Prisoner_1_8>();

        if (prisoner == null)
        {
            Debug.LogError(
                "[Manager_1_8] Prisoner_1_8 컴포넌트가 없습니다."
            );

            Destroy(obj);
            return;
        }

        // 2명씩 같은 속도
        int speedGroup =
            (spawnCount / 2) % 2;

        float assignedSpeed =
            speedGroup == 0
                ? slowSpeed
                : fastSpeed;

        prisoner.Initialize(
            this,
            prisonObj,
            assignedSpeed
        );

        alivePrisoners.Add(prisoner);

        spawnCount++;

        Debug.Log(
            $"[1-8] 죄수 생성 : {spawnCount}번째"
        );
    }


    public void NotifyCaptured(
        Prisoner_1_8 prisoner)
    {
        if (prisoner == null)
            return;

        capturedCount++;

        alivePrisoners.Remove(prisoner);

        demoTriggeredPrisoners.Remove(prisoner);

        if (minigame != null)
        {
            minigame.ReportManualSuccess();
        }
    }


    public void NotifyEscaped(
        Prisoner_1_8 prisoner)
    {
        if (prisoner == null)
            return;

        escapedCount++;

        alivePrisoners.Remove(prisoner);

        demoTriggeredPrisoners.Remove(prisoner);
    }


    public void ClearAllPrisoners()
    {
        for (int i = alivePrisoners.Count - 1;
             i >= 0;
             i--)
        {
            if (alivePrisoners[i] != null)
            {
                Destroy(
                    alivePrisoners[i].gameObject
                );
            }
        }

        alivePrisoners.Clear();

        demoTriggeredPrisoners.Clear();
    }


    // Player 모드에서 클릭했을 때 사용
    public void ActivatePrison()
    {
        if (prisonController == null)
            return;

        prisonController.ActivatePrison();
    }
}