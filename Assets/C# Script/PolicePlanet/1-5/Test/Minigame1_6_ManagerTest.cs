using System.Collections;
using UnityEngine;

public class Minigame1_6_ManagerTest : MiniGameBase
{
    public HandControlerTest handControler;
    public Transform mainHand;
    public GameObject case1_Obj;
    public GameObject case2_Obj;
    public GameObject handSpawn;

    public rythmManager1_5_Test rhythm;

    public float inputCooldown = 0.15f;   // 입력 후 잠금 시간

    public int collideCnt;
    int inputCnt;

    // 입력 잠금
    private bool inputLocked = false;

    // Success/Fail 중복 호출 방지
    private bool gameEnded = false;

    // 목표값
    private const int CASE1_GOAL = 4;
    private const int CASE2_GOAL = 3;

    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "숨은 죄수를 찾아라!";

    private void Start()
    {
        collideCnt = 0;
        gameEnded = false;

        if (case1_Obj != null) case1_Obj.SetActive(false);
        if (case2_Obj != null) case2_Obj.SetActive(false);

        // case 선택
        int randInt = Random.Range(1, 3);
        if (handControler != null) handControler.caseNum = randInt;

        if (randInt == 1 && case1_Obj != null) case1_Obj.SetActive(true);
        else if (randInt == 2 && case2_Obj != null) case2_Obj.SetActive(true);

        // 판정 이벤트 구독
        if (rhythm != null)
        {
            rhythm.OnPlayerJudged += HandleJudgement;
        }
        else
        {
            Debug.Log("rhythm 매니저 연결 안됨");
        }
    }

    public override void StartGame()
    {
        // 미니게임 시작 시 초기화(필요한 것만)
        collideCnt = 0;
        inputLocked = false;
        gameEnded = false;
    }

    private void OnDestroy()
    {
        if (rhythm != null)
        {
            rhythm.OnPlayerJudged -= HandleJudgement;
        }
    }

    private void Update()
    {
        if (gameEnded) return;

        // 입력
        if (Input.GetMouseButtonDown(0))
        {
            TryInput();
            inputCnt++;
        }
        if(handControler.caseNum==1&&inputCnt>=4)
        {
            inputLocked = true;
        }
        if (handControler.caseNum == 2 && inputCnt >= 3)
        {
            inputLocked = true;
        }

        
        CheckSuccessCondition();

        
    }

    private void TryInput()
    {
        // (연타 방지)
        if (inputLocked) return;

        SpawnHand();

        // 판정 요청
        if (rhythm != null)
            rhythm.ReceivePlayerInput("Input");

        // 입력 후 일정 시간 잠금
        StartCoroutine(InputCooldownRoutine());
    }

    private IEnumerator InputCooldownRoutine()
    {
        inputLocked = true;
        yield return new WaitForSeconds(inputCooldown);
        inputLocked = false;
    }

    // 리듬매니저 판정 결과 
    private void HandleJudgement(MiniGameBase.JudgementResult judgement)
    {
        if (gameEnded) return;

        if (judgement == MiniGameBase.JudgementResult.Perfect ||
            judgement == MiniGameBase.JudgementResult.Good)
        {
            // 판정 성공 시 추가 처리 원하면 여기서
            // (요청대로 Success/Fail 판정은 collideCnt 기준으로만 연결)
        }
        else
        {
            //Fail();
        }
    }

    private void CheckSuccessCondition()
    {
        if (handControler == null) return;

        if (handControler.caseNum == 1 && collideCnt >= CASE1_GOAL)
        {
            EndSuccess();
        }
        else if (handControler.caseNum == 2 && collideCnt >= CASE2_GOAL)
        {
            EndSuccess();
        }
    }

    // MiniGameBase 성공 처리 연결
    private void EndSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        Success();
    }

   

   
    public override void Fail()
    {
        if (gameEnded) return;
        gameEnded = true;

        base.Fail();
    }

    private void SpawnHand()
    {
        if (handSpawn == null || mainHand == null)
        {
            Debug.Log("handSpawn 또는 mainHand 할당 안됨");
            return;
        }

        Vector2 spawnPos = new Vector2(mainHand.position.x, -1f);
        Instantiate(handSpawn, spawnPos, Quaternion.identity);
    }
}
