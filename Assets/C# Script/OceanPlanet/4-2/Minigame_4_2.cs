using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_4_2 : MiniGameBase
{
    [Header("Dot Prefabs")]
    public GameObject yellowDotPrefab;
    public GameObject redDotPrefab;
    public Transform startPoint;

    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "적 요격";

    protected override string MinigameExplain => "우측에서 음파가 칸 단위로 다가옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "음파가 판정구역에 도달했을 때 정확히 클릭하면 점수를 획득합니다."
    };

    public override void OnRhythmEvent(string action)
    {
        switch (action)
        {
            case "Show1":
                Instantiate(yellowDotPrefab, startPoint.position, Quaternion.identity);
                break;

            case "Show2":
                Instantiate(redDotPrefab, startPoint.position, Quaternion.identity);
                break;
        }
    }


}
