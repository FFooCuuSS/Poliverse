using System.Collections;
using UnityEngine;

public class Minigame1_6_Manager_remake : MiniGameBase
{
    public HandControlerTest handControler;
    public Transform mainHand;
    public GameObject case1_Obj;
    public GameObject case2_Obj;
    public GameObject handSpawn;

    public rythmManager1_5_Test rhythm;

    public float inputCooldown = 0.15f;   // 입력 후 잠금 시간

    public int collideCnt;
    int judgeCnt;
    int inputCnt;

    // 입력 잠금
    private bool cooldownLocked = false;
    private bool maxInputLocked = false;

    // Success/Fail 중복 호출 방지
    private bool gameEnded = false;

    // 목표값
    private const int CASE1_GOAL = 4;
    private const int CASE2_GOAL = 3;

    protected override float TimerDuration => 12f;
    protected override string MinigameExplain => "숨은 죄수를 찾아라!";

    private void Start()
    {
        judgeCnt = 0;
        collideCnt = 0;
        inputCnt = 0;
        cooldownLocked = false;
        maxInputLocked = false;
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
       // Debug.Log(handControler.caseNum);

    }

    public override void StartGame()
    {
        collideCnt = 0;
        judgeCnt = 0;
        inputCnt = 0;

        cooldownLocked = false;
        maxInputLocked = false;
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
       // Debug.Log("Update 들어옴"); // 1) Update 자체가 도는지
       // if (handControler == null) { Debug.Log("handControler NULL"); return; } // 2) null 체크
       // Debug.Log("caseNum=" + handControler.caseNum); // 3) 값 확인
        if (gameEnded) return;

        // 영구 잠금이면 입력 자체를 막음
        if (maxInputLocked) return;
        

        // case별 입력 제한
        if (handControler.caseNum == 1)
        {
           // Debug.Log("inputCnt" + inputCnt + "maxInputLock: " + maxInputLocked);

            if (inputCnt >= 4) maxInputLocked = true;
            if (mainHand.position.x < -12)
                CheckSuccessCondition();
        }
        else if (handControler.caseNum == 2)
        {
            if (inputCnt >= 3) maxInputLocked = true;
           // Debug.Log("inputCnt" + inputCnt + "maxInputLock: " + maxInputLocked);

            if (mainHand.position.x < -14.5)
                CheckSuccessCondition();
        }
        if (cooldownLocked == false && maxInputLocked==false)
        {
            if (Input.GetMouseButtonDown(0))
            {
                inputCnt++;

                TryInput();
            }

        }
    }

    private void TryInput()
    {
        // 안전장치
        if (cooldownLocked || maxInputLocked) return;

        SpawnHand();

        if (rhythm != null)
            rhythm.ReceivePlayerInput("Input");

        

        StartCoroutine(InputCooldownRoutine());
    }

    private IEnumerator InputCooldownRoutine()
    {
        cooldownLocked = true;
        yield return new WaitForSeconds(inputCooldown);
        cooldownLocked = false;
    }

    // 리듬매니저 판정 결과 
    private void HandleJudgement(MiniGameBase.JudgementResult judgement)
    {
        if (gameEnded) return;

        if (judgement == MiniGameBase.JudgementResult.Perfect ||
            judgement == MiniGameBase.JudgementResult.Good)
        {
            judgeCnt++;
        }
        else
        {
            //Fail();
        }
    }

    private void CheckSuccessCondition()
    {
        if (handControler == null) return;

        if (handControler.caseNum == 1 && collideCnt >= CASE1_GOAL )
        {
            EndSuccess();
        }
        else if (handControler.caseNum == 2 && collideCnt >= CASE2_GOAL )
        {
            EndSuccess();
        }
        else
        {
            Fail();
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
