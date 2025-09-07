using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame2_6 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "좌우로 피해라!";

    public override void Success()
    {
        base.Success();
    }

    public override void Fail()
    {
        base.Fail();
    }
}
