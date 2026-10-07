using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_11 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 20f;
    protected override string MinigameTitle => "마카롱 쌓기";

    protected override string MinigameExplain => "타이밍에 맞춰 화면을 터치하여 마카롱을 집으세요.";

    private bool ended;

    protected override bool UseRhythmJudgementScore => false;

    protected override int ManualTotalNodeCount => -1;

    private bool isDemoMode;

    [SerializeField]
    private Fork_2_11 fork;

    [SerializeField]
    private MacaroonSpawn spawner;

    [System.Serializable]
    public class MacaronPattern
    {
        public string name;

        // 0~4 = 1~5박 슬롯
        public int[] activeSlots;
    }

    [Header("패턴 사전 (0~4번 인덱스로 참조됨)")]
    [SerializeField]
    private List<MacaronPattern> patterns =
        new List<MacaronPattern>
        {
            /* 0 */
            new MacaronPattern
            {
                name = "전체 등장",
                activeSlots = new int[] { 0, 1, 2, 3, 4 }
            },

            /* 1 */
            new MacaronPattern
            {
                name = "1,3,5박만",
                activeSlots = new int[] { 0, 2, 4 }
            },

            /* 2 */
            new MacaronPattern
            {
                name = "1,2,4박만",
                activeSlots = new int[] { 0, 1, 3 }
            },

            /* 3 */
            new MacaronPattern
            {
                name = "2,3,4,5박",
                activeSlots = new int[] { 1, 2, 3, 4 }
            },

            /* 4 */
            new MacaronPattern
            {
                name = "1,5박만",
                activeSlots = new int[] { 0, 4 }
            },
        };

    [Header("등장 순서")]
    [SerializeField]
    private List<int> patternOrder =
        new List<int> { 0, 2, 1, 3, 4 };

    private int orderCursor = 0;

    private int accumulatedTotalNodeCount = 0;

    public bool IsEnded => ended;


    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        Debug.Log(
            $"[Minigame_2_11] Demo Mode = {isDemoMode}"
        );

        if (fork != null)
        {
            fork.SetDemoMode(isDemo);
        }
        else
        {
            Debug.LogWarning(
                "[Minigame_2_11] Fork_2_11이 연결되지 않았습니다."
            );
        }
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        orderCursor = 0;
        accumulatedTotalNodeCount = 0;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended)
            return;

        if (string.IsNullOrEmpty(action))
            return;

        action = action.Trim();

        Debug.Log(
            $"[Minigame_2_11] 리듬 이벤트 = {action}"
        );

        switch (action)
        {
            case "Show":

                // Demo / Player 모두 마카롱 생성
                SpawnNextRound();

                break;
        }
    }

    public void SpawnNextRound()
    {
        if (ended)
            return;

        if (patternOrder == null || orderCursor >= patternOrder.Count)
        {
            FinishAllRounds();
            return;
        }

        int patternIndex = patternOrder[orderCursor];
        orderCursor++;

        if (patterns == null || patternIndex < 0 || patternIndex >= patterns.Count)
        {
            Debug.LogWarning($"[Minigame_2_11] 잘못된 패턴 인덱스 = {patternIndex}");
            FinishAllRounds();
            return;
        }

        MacaronPattern pattern = patterns[patternIndex];

        int spawnedCount = spawner.SpawnMacarons(pattern.activeSlots);

        accumulatedTotalNodeCount += spawnedCount;

        SetRuntimeTotalNodeCount(accumulatedTotalNodeCount);

        Debug.Log($"[Minigame_2_11] 패턴 '{pattern.name}' ({orderCursor}/{patternOrder.Count}) 스폰 = {spawnedCount}");
    }


    public override void OnPlayerInput(string action = null)
    {
        if (ended)
            return;

        if (isDemoMode)
            return;

        Debug.Log(
            "[Minigame_2_11] 플레이어 클릭 입력"
        );
    }


    public void MacaronSuccess()
    {
        if (ended)
            return;

        if (isDemoMode)
            return;

        ReportManualSuccess();

        Debug.Log(
            "마카롱 쌓기 성공"
        );
    }

    public void MacaronFail()
    {
        if (ended)
            return;

        if (isDemoMode)
            return;

        ReportManualFail();

        Debug.Log(
            "마카롱 쌓기 실패"
        );
    }


    private void FinishAllRounds()
    {
        if (ended)
            return;

        ended = true;

        Debug.Log(
            "[Minigame_2_11] " +
            "모든 패턴 진행 완료 - 미니게임 종료"
        );

        Success();
    }


    private MacaronPattern NextPattern()
    {
        if (patterns == null ||
            patterns.Count == 0)
        {
            Debug.LogWarning(
                "[Minigame_2_11] 등록된 패턴이 없습니다."
            );

            return new MacaronPattern
            {
                name = "기본",
                activeSlots = null
            };
        }

        if (patternOrder == null ||
            patternOrder.Count == 0)
        {
            return patterns[0];
        }

        int patternIndex =
            patternOrder[
                orderCursor % patternOrder.Count
            ];

        orderCursor++;

        if (patternIndex < 0 ||
            patternIndex >= patterns.Count)
        {
            return patterns[0];
        }

        return patterns[patternIndex];
    }
}