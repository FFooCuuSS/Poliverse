using System;
using UnityEngine;

public class Minigame_3_12 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain =>
        "박자에 맞춰 탈출하세요!";

    // 이 미니게임은 RhythmManager의 Perfect/Good/Miss 판정을 사용하지 않는다.
    // 각 박자마다 안전하면 ManualSuccess,
    // 발각되면 ManualFail을 직접 보고한다.
    protected override bool UseRhythmJudgementScore => false;

    // 실제로 진행한 박자 수를 최종 총 노드 수로 사용한다.
    protected override int ManualTotalNodeCount => -1;

    // ManualFail로 보고된 횟수만 Miss로 처리한다.
    protected override bool AutoFillRemainingAsMiss => false;

    [Header("References")]
    [SerializeField] private GridBoard_3_12 board;
    [SerializeField] private PlayerGridMover_3_12 player;
    [SerializeField] private Enemy_3_12[] enemies;

    [Header("Rhythm Event")]
    [Tooltip("CSV에서 한 턴을 진행시키는 이벤트 이름")]
    [SerializeField] private string beatActionName = "Move";

    private bool ended;

    protected override void Awake()
    {
        base.Awake();

        if (board == null)
            board = GetComponentInChildren<GridBoard_3_12>(true);

        if (player == null)
            player = GetComponentInChildren<PlayerGridMover_3_12>(true);

        if (enemies == null || enemies.Length == 0)
            enemies = GetComponentsInChildren<Enemy_3_12>(true);
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;

        if (board == null)
        {
            Debug.LogError("[3-12] GridBoard_3_12가 없습니다.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("[3-12] PlayerGridMover_3_12가 없습니다.");
            return;
        }

        player.Initialize(board);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
                enemies[i].Initialize(board);
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended || IsInputLocked)
            return;

        if (string.IsNullOrWhiteSpace(action))
            return;

        if (!string.Equals(
                action.Trim(),
                beatActionName,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ResolveBeat();
    }

    private void ResolveBeat()
    {
        if (board == null || player == null)
            return;

        // 1. 플레이어가 미리 예약한 스와이프를 실행한다.
        player.ResolveBeatMove();

        // 2. 같은 박자에 적들도 이동하거나 회전한다.
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
                enemies[i].ResolveBeatAction();
        }

        // 루트 Transform과 Collider 위치를 즉시 동기화한다.
        Physics2D.SyncTransforms();

        // 3. 모든 적의 시야를 검사한다.
        bool caught = IsPlayerCaught();

        if (caught)
        {
            // 점수상 Miss 1회.
            // 미니게임 자체를 실패시키지는 않는다.
            ReportManualFail();
            player.Revealed();

            Debug.Log("[3-12] 발각: ManualFail");
        }
        else
        {
            // 안전하게 넘긴 박자 1회.
            ReportManualSuccess();

            Debug.Log("[3-12] 안전: ManualSuccess");
        }

        // 4. 발각 여부와 별개로 출구 도착을 판정한다.
        if (player.CurrentCell == board.GoalCell)
            CompleteGame();
    }

    private bool IsPlayerCaught()
    {
        if (player.PlayerCollider == null)
            return false;

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy_3_12 enemy = enemies[i];

            if (enemy != null &&
                enemy.CanSeePlayer(player.PlayerCollider))
            {
                // 여러 적에게 동시에 보여도
                // 한 박자에는 ManualFail 한 번만 발생한다.
                return true;
            }
        }

        return false;
    }

    public void Succeed()
    {
        CompleteGame();
    }

    private void CompleteGame()
    {
        if (ended)
            return;

        ended = true;
        player.SetInputEnabled(false);

        Success();
    }

    private void OnDisable()
    {
        if (player != null)
            player.SetInputEnabled(false);
    }
}