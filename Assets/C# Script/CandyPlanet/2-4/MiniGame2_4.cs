using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame2_4 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "같은 색끼리 옮겨담아라!";

    public override void Success()
    {
        base.Success();
    }

    public override void Fail()
    {
        base.Fail();
    }
}
