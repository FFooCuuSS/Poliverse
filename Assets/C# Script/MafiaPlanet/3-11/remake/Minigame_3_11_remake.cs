using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class Minigame_3_11_remake : MiniGameBase, IPracticeDemoInput
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "후르츠 닌자";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;


    private bool finished = false;

    [Header("데모 설정")]
    [SerializeField] private GameManager_3_11 gameManager;

    private bool isDemoMode;

    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager_3_11>();

        if (gameManager != null)
            gameManager.SetDemoMode(isDemoMode);

        Debug.Log("[3-11] 데모 모드: " + isDemoMode);
    }

    private void Start()
    {
        Debug.Log("[3-11] Minigame3_11remake 시작");

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager_3_11>();

        if (gameManager != null)
            gameManager.SetDemoMode(isDemoMode);

        base.StartGame();
    }


    private void Update()
    {
        if (finished) return;

       
    }

    
}
