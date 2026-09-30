using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class Minigame_3_11_remake : MiniGameBase
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "미니게임 3-11 설명을 여기에 넣기";
    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;


    private bool finished = false;

    private void Start()
    {
        Debug.Log("[3-11] Minigame3_11remake 시작");
        base.StartGame();
    }


    private void Update()
    {
        if (finished) return;

       
    }

    
}
