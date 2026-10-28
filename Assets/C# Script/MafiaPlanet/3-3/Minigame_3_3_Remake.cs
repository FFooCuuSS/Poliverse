using UnityEngine;

public class Minigame_3_3_Remake : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "문따기";

    protected override string MinigameExplain => "바깥쪽 고리부터 차례대로 터치해 각 고리의 구멍을 일직선으로 맞추세요.";

    [Header("Manager")]
    [SerializeField] private Manager_3_3 manager;

    private bool ended;
    private bool inputOpen;

    private bool isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        if (manager != null)
            manager.SetDemoMode(isDemo);

        // 드래그로 열쇠를 움직이는 플레이어 입력도 차단
        BlockingDirectionalDrag[] dragObjects =
            FindObjectsOfType<BlockingDirectionalDrag>();

        foreach (var drag in dragObjects)
        {
            if (drag != null)
                drag.SetDemoMode(isDemo);
        }

        Debug.Log("[3-3] 데모 모드: " + isDemoMode);
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;

        manager?.OnMinigameStart(this);
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        switch (action)
        {
            case "Show":
                Debug.Log("[3-3] 라운드 시작");
                inputOpen = true;
                manager?.StartNextRound();
                break;

            case "Move":
                Debug.Log("[3-3] 입력 종료");
                inputOpen = false;
                manager?.CloseInput();
                break;
        }
    }

    private void Update()
    {
        if (ended) return;
        if (!inputOpen) return;
        if (isDemoMode) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[3-3] 클릭");
            manager?.OnPlayerClick();
        }
    }

    public void ForceCloseInput()
    {
        inputOpen = false;
    }

    public void FinishGame(int successCount)
    {
        if (ended) return;

        ended = true;
        inputOpen = false;

        Debug.Log($"[3-3] 종료 / 성공 횟수: {successCount}");
        Success();
    }
}