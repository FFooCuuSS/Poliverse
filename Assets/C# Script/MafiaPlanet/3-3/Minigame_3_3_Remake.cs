using UnityEngine;

public class Minigame_3_3_Remake : MiniGameBase
{
    protected override float TimerDuration => 20f;
    protected override string MinigameExplain => "타이밍에 맞춰 열쇠를 맞춰라!";

    [Header("Manager")]
    [SerializeField] private Manager_3_3 manager;

    private bool ended;
    private bool inputOpen = false;

    private void Start()
    {
        StartGame();
    }

    public override void StartGame()
    {
        ended = false;

        manager?.OnMinigameStart(this);

        inputOpen = true;
        manager?.StartNextRound();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;

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
                break;
        }
    }

    private void Update()
    {
        if (!inputOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[3-3] 클릭");
            manager?.OnPlayerClick();
        }
    }

    public void FinishGame(int successCount)
    {
        if (ended) return;

        ended = true;
        Debug.Log($"[3-3] 종료 / 성공 횟수: {successCount}");
    }
}