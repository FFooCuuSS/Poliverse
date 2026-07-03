using UnityEngine;

public class Minigame_3_3_Remake : MiniGameBase
{
    protected override float TimerDuration => 20f;
    protected override string MinigameExplain => "타이밍에 맞춰 열쇠를 맞춰라!";

    [Header("Manager")]
    [SerializeField] private Manager_3_3 manager;

    private bool ended;
    private bool inputOpen;


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