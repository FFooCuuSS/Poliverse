using System.Collections.Generic;

public class Minigame_1_4 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "리듬에 맞춰 악세서리를 제거하세요.";

    private List<Accessory> orderedAccessories;
    private bool hasFailed = false;

    public void SetAccessoryOrder(List<Accessory> accessories)
    {
        orderedAccessories = accessories;

        foreach (var acc in orderedAccessories)
            acc.Init(this);
    }

    public override void StartGame()
    {
        base.StartGame();
        hasFailed = false;
    }

    public override void OnPlayerInput(string action = null)
    {
        if (IsInputLocked || hasFailed) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (hasFailed) return;

        if (judgement == JudgementResult.Miss)
        {
            hasFailed = true;
            //Fail();
            return;
        }

        RemoveNextAccessory();

        if (AllAccessoriesRemoved())
        {
            Success();
        }
    }

    private void RemoveNextAccessory()
    {
        foreach (var acc in orderedAccessories)
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
        foreach (var acc in orderedAccessories)
        {
            if (!acc.IsRemoved) return false;
        }
        return true;
    }
}
