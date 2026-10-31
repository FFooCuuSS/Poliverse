using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class Minigame_3_11_remake : MiniGameBase, IPracticeDemoInput
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "서류 담기";

    protected override string MinigameExplain => "서류 혹은 폭탄이 날아옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "폭탄은 피하고, 서류가 박스에 도착하면 화면을 터치하세요."
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
