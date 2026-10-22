using UnityEngine;

public class Minigame_4_1 : MiniGameBase
{
    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "암구호 두드리기";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };
}
