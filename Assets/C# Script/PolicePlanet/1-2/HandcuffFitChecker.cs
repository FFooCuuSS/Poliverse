using System.Collections.Generic;
using UnityEngine;

public class HandcuffFitChecker : MonoBehaviour
{
    public GameObject stage_1_2;
    private Minigame_1_2 minigame_1_2;

    [SerializeField] private GameObject objectToDestroy_1;
    [SerializeField] private GameObject objectToDestroy_2;
    [SerializeField] private GameObject objectToSpawn;

    [SerializeField] private List<CircleCollider2D> handColliders;

    private CircleCollider2D cuffCollider;
    private DragAndDrop dragAndDrop;

    private bool isSnapped = false;
    private CircleCollider2D snappedHand = null;
    private static bool isGameEnded = false;

    private void Start()
    {
        minigame_1_2 = stage_1_2.GetComponent<Minigame_1_2>();
        cuffCollider = GetComponent<CircleCollider2D>();
        dragAndDrop = GetComponent<DragAndDrop>();
    }

    public static void ResetRound()
    {
        isGameEnded = false;

        // 씬에 남아있는 기존 수갑들도 상태 초기화(활성 유지 전제)
        var all = FindObjectsOfType<HandcuffFitChecker>();
        foreach (var c in all)
        {
            c.isSnapped = false;
            c.snappedHand = null;

            if (c.dragAndDrop != null) c.dragAndDrop.enabled = true;
            if (c.cuffCollider != null) c.cuffCollider.enabled = true;

            // 위치 리셋이 필요하면, 여기서 원위치 저장/복구 로직 추가
            // (현재 코드는 원위치 변수가 없으니 필요하면 추가해라)
        }
    }

    private void Update()
    {
        if (!isSnapped && !isGameEnded)
        {
            CheckAndSnap();
        }
    }

    private void CheckAndSnap()
    {
        foreach (var handcol in handColliders)
        {
            if (handcol == null || !handcol.enabled) continue;

            if (cuffCollider.bounds.Intersects(handcol.bounds))
            {
                transform.position = handcol.bounds.center;
                isSnapped = true;
                snappedHand = handcol;

                if (dragAndDrop != null) dragAndDrop.enabled = false;

                GameClearCheck();
                break;
            }
        }
    }

    private void GameClearCheck()
    {
        if (isGameEnded) return;

        var all = FindObjectsOfType<HandcuffFitChecker>();

        foreach (var cuff in all)
        {
            if (!cuff.isSnapped)
                return;
        }

        bool stateOk = (HandcuffSequenceController.Instance != null &&
                        HandcuffSequenceController.Instance.curState == HandcuffSequenceController.State.PlayerDrag);

        bool inputOk = (minigame_1_2 != null && minigame_1_2.IsInputWindowOpen);

        if (!stateOk || !inputOk)
        {
            // 타이밍 안 맞음 → 즉시 실패
            isGameEnded = true;
            if (minigame_1_2 != null) minigame_1_2.Failure();
            return;
        }

        if (all.Length >= 2 && all[0].snappedHand != all[1].snappedHand)
        {
            isGameEnded = true;

            // 리듬 판정으로 넘김 (MinigameBase → RhythmManager)
            if (minigame_1_2 != null) minigame_1_2.OnPlayerInput("Input");

            if (objectToDestroy_1 != null) objectToDestroy_1.SetActive(false);
            if (objectToDestroy_2 != null) objectToDestroy_2.SetActive(false);
            if (objectToSpawn != null) objectToSpawn.SetActive(true);
        }
        else
        {
            // 같은 손에 두 개 → 실패
            isGameEnded = true;
            if (minigame_1_2 != null) minigame_1_2.Failure();
        }
    }

    public void ForceSnapToHand(CircleCollider2D handCol)
    {
        transform.position = handCol.bounds.center;
        isSnapped = true;
        snappedHand = handCol;

        if (dragAndDrop != null)
            dragAndDrop.enabled = false;
        if (cuffCollider != null)
            cuffCollider.enabled = false;

        GameClearCheck();
    }
}