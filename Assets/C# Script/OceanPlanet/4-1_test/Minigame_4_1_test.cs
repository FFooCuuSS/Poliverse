using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_4_1_test : MiniGameBase
{
    [Header("Effect Objects")]
    [SerializeField]
    private ShowEffact playerEffect;

    [SerializeField]
    private ShowEffact doorEffect;

    [Header("Test Input")]
    [SerializeField]
    private KeyCode playerInputKey = KeyCode.Space;
    protected override float TimerDuration => 15f;

    protected override string MinigameTitle =>
        "암구호 두드리기";

    protected override string MinigameExplain =>
        "정해진 타이밍에 문을 두드리면 점수를 획득합니다.";

    private void Update()
    {
        if (Input.GetKeyDown(playerInputKey))
        {
            OnPlayerInput();
        }
    }


    public override void OnRhythmEvent(string action)
    {
        if (action == "Show")

        {
            doorEffect.BlinkEffect();
        }

        if (action == "Input")
        {
            
            // 입력 가능 상태 만들기
        }
    }
    public void OnPlayerInput()
    {
        if (IsDemoMode || IsInputLocked)
        {
            return;
        }

        playerEffect.BlinkEffect();

        if (rhythmManager != null)
        {
            rhythmManager.ReceivePlayerInput();
        }
    }
}
