using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_11 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "쌓아보세요!";

    private bool ended;

    protected override bool UseRhythmJudgementScore => false;

    protected override int ManualTotalNodeCount => -1;

    [SerializeField] private MacaroonSpawn spawner;

    [System.Serializable]
    public class MacaronPattern
    {
        public string name;
        // 0~4 = 1~5박에 대응하는 슬롯 인덱스
        public int[] activeSlots;
    }

    [Header("패턴 사전 (0~4번 인덱스로 참조됨)")]
    [SerializeField]
    private List<MacaronPattern> patterns = new List<MacaronPattern>
    {
        /* 0 */ new MacaronPattern { name = "전체 등장",   activeSlots = new int[] { 0, 1, 2, 3, 4 } },
        /* 1 */ new MacaronPattern { name = "1,3,5박만",   activeSlots = new int[] { 0, 2, 4 } },
        /* 2 */ new MacaronPattern { name = "1,2,4박만",   activeSlots = new int[] { 0, 1, 3 } },
        /* 3 */ new MacaronPattern { name = "2,3,4,5박",   activeSlots = new int[] { 1, 2, 3, 4 } },
        /* 4 */ new MacaronPattern { name = "1,5박만",     activeSlots = new int[] { 0, 4 } },
    };

    [Header("등장 순서 (위 patterns 리스트의 인덱스를 순서대로 나열, 끝까지 가면 반복)")]
    [SerializeField]
    private List<int> patternOrder = new List<int> { 0, 2, 1, 3, 4 };

    // patternOrder에서 다음으로 읽을 위치
    private int orderCursor = 0;

    private int accumulatedTotalNodeCount = 0;
    public bool IsEnded => ended;

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        orderCursor = 0;
        accumulatedTotalNodeCount = 0;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        switch (action)
        {
            case "Show":
                SpawnNextRound();
                break;
        }
    }
    public void SpawnNextRound()
    {
        if (ended) return;

        // 등장 순서 리스트를 끝까지 다 돌았으면 더 이상 진행하지 않고 종료
        if (patternOrder == null || orderCursor >= patternOrder.Count)
        {
            FinishAllRounds();
            return;
        }

        int patternIndex = patternOrder[orderCursor];
        orderCursor++;

        if (patterns == null || patternIndex < 0 || patternIndex >= patterns.Count)
        {
            Debug.LogWarning($"[Minigame_2_11] patternOrder에 잘못된 인덱스({patternIndex})가 있습니다. 종료 처리.");
            FinishAllRounds();
            return;
        }

        MacaronPattern pattern = patterns[patternIndex];

        int spawnedCount = spawner.SpawnMacarons(pattern.activeSlots);

        accumulatedTotalNodeCount += spawnedCount;
        SetRuntimeTotalNodeCount(accumulatedTotalNodeCount);

        Debug.Log($"[Minigame_2_11] 패턴 '{pattern.name}' 적용 ({orderCursor}/{patternOrder.Count}), 스폰={spawnedCount}, 누적 총량={accumulatedTotalNodeCount}");
    }

    // patternOrder를 끝까지 다 돌았을 때 호출 - 미니게임 종료
    private void FinishAllRounds()
    {
        if (ended) return;
        ended = true;

        Debug.Log("[Minigame_2_11] 모든 패턴 진행 완료 - 미니게임 종료");

        Success(); // 전체 패턴을 다 소화하면 성공 처리. 성공/실패 기준을 다르게 하고 싶으면 여기만 바꾸면 됨
    }
    // patternOrder를 순서대로 하나씩 읽어서, 거기 적힌 인덱스로 patterns에서 패턴을 꺼낸다.
    // 마지막 인덱스까지 다 읽으면 orderCursor가 다시 0으로 돌아가 처음부터 반복.
    private MacaronPattern NextPattern()
    {
        if (patterns == null || patterns.Count == 0)
        {
            Debug.LogWarning("[Minigame_2_11] 등록된 패턴이 없습니다. 기본(전체) 패턴 사용.");
            return new MacaronPattern { name = "기본", activeSlots = null };
        }

        if (patternOrder == null || patternOrder.Count == 0)
        {
            Debug.LogWarning("[Minigame_2_11] patternOrder가 비어있습니다. patterns[0] 사용.");
            return patterns[0];
        }

        int patternIndex = patternOrder[orderCursor % patternOrder.Count];

        orderCursor++;

        if (patternIndex < 0 || patternIndex >= patterns.Count)
        {
            Debug.LogWarning($"[Minigame_2_11] patternOrder에 잘못된 인덱스({patternIndex})가 있습니다. patterns[0] 사용.");
            return patterns[0];
        }

        return patterns[patternIndex];
    }

    public override void OnPlayerInput(string action = null)
    {
        if (ended)
            return;

        Debug.Log("플레이어 클릭 입력");
    }

    public void MacaronSuccess()
    {
        ReportManualSuccess();

        Debug.Log("마카롱 쌓기 성공");
    }

    public void MacaronFail()
    {
        ReportManualFail();

        Debug.Log("마카롱 쌓기 실패");
    }
}