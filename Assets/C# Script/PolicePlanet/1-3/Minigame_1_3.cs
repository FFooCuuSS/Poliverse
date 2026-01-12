using UnityEngine;

public class Minigame_1_3 : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "조심해라!";

    private float currentTime;
    private bool isTimerRunning;

    private void Start()
    {
        StartGame();
    }
    public override void StartGame()
    {
        base.StartGame();
        Debug.Log("StartGame 호출됨");
        currentTime = TimerDuration;
        isTimerRunning = true;
    }

    private void Update()
    {
        RunTimer();
    }

    private void RunTimer()
    {
        if (!isTimerRunning) return;
        if (IsSuccess) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            isTimerRunning = false;
            base.Success();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Tap":
                break;
            case "Hold":
                break;
            case "Swipe":
                break;
        }
    }
}
