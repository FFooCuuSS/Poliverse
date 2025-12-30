using System.Collections.Generic;

public class Minigame_1_4 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "리듬에 맞춰 악세서리를 제거하세요.";

    private List<Accessory> accessories = new List<Accessory>();
    private bool hasFailed = false;

    public void RegisterAccessory(Accessory acc)
    {
        acc.Init(this);
        accessories.Add(acc);
    }

    public override void StartGame()
    {
        base.StartGame();
        hasFailed = false;
    }

    // 악세서리 → 미니게임
    public override void OnPlayerInput(string action = null)
    {
        if (IsInputLocked || hasFailed) return;
        base.OnPlayerInput(action); // RhythmManager로 전달
    }

    // RhythmManager → 미니게임
    public override void OnJudgement(JudgementResult judgement)
    {
        if (hasFailed) return;

        if (judgement == JudgementResult.Miss)
        {
            hasFailed = true;
            Fail();
            return;
        }

        // Perfect / Good
        RemoveNextAccessory();

        if (AllAccessoriesRemoved())
        {
            Success();
        }
    }

    private void RemoveNextAccessory()
    {
        foreach (var acc in accessories)
        {
            if (!acc.IsRemoved)
            {
                acc.Remove();
                break;
            }
        }
    }

    private bool AllAccessoriesRemoved()
    {
        foreach (var acc in accessories)
        {
            if (!acc.IsRemoved) return false;
        }
        return true;
    }
}
