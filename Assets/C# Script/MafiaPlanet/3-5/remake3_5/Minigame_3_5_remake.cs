using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Minigame_3_5_remake : MiniGameBase, IPracticeDemoInput
{
    public GameObject scope;
    public ScopeMover scopeMover;
   // public CameraFollow3_5 cameraFollow;
    bool moveStarted = false;
    private float timechecker = 0f;

    protected override float TimerDuration => 15f;

    protected override string MinigameTitle => "문따기";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };

    [Header("데모")]
    [SerializeField] private ScopeShooter scopeShooter;
    private bool isDemoMode = false;

    private void Awake()
    {
        FindScopeShooter();
    }

    private void Start()
    {
        if (scope != null)
            scope.SetActive(false);

        StartGame();
        moveStarted = true;

        // 데모 매니저가 설정한 모드를 적용
        ApplyDemoMode();
    }

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        ApplyDemoMode();

        Debug.Log("[3-5] 데모 모드: " + isDemoMode);
    }

    private void FindScopeShooter()
    {
        if (scope != null && scopeShooter == null)
        {
            scopeShooter =
                scope.GetComponentInChildren<ScopeShooter>(true);
        }
    }

    private void ApplyDemoMode()
    {
        FindScopeShooter();

        if (scopeShooter != null)
        {
            scopeShooter.SetDemoMode(isDemoMode);
        }
        else
        {
            Debug.LogError(
                "[3-5] ScopeShooter를 찾을 수 없습니다. Inspector를 확인하세요."
            );
        }
    }


    public override void StartGame()
    {
        Debug.Log("[3-4] StartGame called");
        base.StartGame();

    }

    private void Update()
    {
        timechecker += Time.deltaTime;

        if (timechecker >= 3.6f && moveStarted)
        {
            if (scope != null)
                scope.SetActive(true);

            if (scopeMover != null)
                scopeMover.StartMove();

            moveStarted = false;
        }
    }

    // 적을 모두 처치했는지 확인한 후 성공 처리
    public IEnumerator CheckDemoCompletion()
    {
        // Destroy는 프레임 끝에 반영되므로 한 프레임 대기
        yield return null;

        if (!isDemoMode)
            yield break;

        GameObject[] remainingEnemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        Debug.Log(
            "[3-5] 남은 적 수: " + remainingEnemies.Length
        );

        if (remainingEnemies.Length == 0)
        {
            Debug.Log("[3-5] 데모 완료: 성공 처리");
            Success();
        }
    }

}
