using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_7 : MiniGameBase
{
    public GameObject ProhibitSpawner;
    private ProhibitedItemSpawner1_7 prohibitedItemSpawner1_7;

    public HoldCheck1_7 holdJudge;

    private bool isHolding = false;

    private int holdCount = 0;
    public int maxHoldCount = 4;

    private float holdTimer = 0f;
    private float holdTimeout = 2f;

    private bool hasMissed = false;

    public Transform basket;


    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "금지야!";


    private void Start()
    {
        prohibitedItemSpawner1_7 = ProhibitSpawner.GetComponent<ProhibitedItemSpawner1_7>();
    }

    void Update()
    {
        GameObject prisoner = FindObjectOfType<PrisonerController1_7>()?.gameObject;
        if (prisoner == null) return;

        // 화면 중앙 근처 체크
        Vector3 screenPos = Camera.main.WorldToViewportPoint(prisoner.transform.position);
        bool isNearCenter = screenPos.x > 0.45f && screenPos.x < 0.55f;

        // 중앙 근처에 있지 않으면 Hold UI 숨김
        if (!isNearCenter && holdJudge != null)
        {
            holdJudge.HideHoldUI();
        }

        // Hold 입력 감지 (예: 리듬 이벤트에 연결되어 있을 수도 있음)
        // 여기서는 그냥 예시로 Space 키로 테스트 가능
        if (isNearCenter && Input.GetKeyDown(KeyCode.Space))
        {
            HandleHold();
        }
    }

    public override void StartGame()
    {
        hasMissed = false;
        holdCount = 0;

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public override void Success()
    {
        base.Success();
        
    }

    public override void Fail()
    {
        base.Fail();
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        // 이건 나중에 개별 미니게임에서 override하는 형태로
        switch (action)
        {
            case "Tap":
                Debug.Log("Tap");
                //ShowTapPrompt();
                break;

            case "Hold":
                HandleHold();
                //Debug.Log("Hold");
                //ShowHoldPrompt();
                break;

            case "Swipe":
                Debug.Log("Swipe");
                //ShowSwipePrompt();
                break;
        }
    }

    private void HandleHold()
    {
        GameObject prisoner = FindObjectOfType<PrisonerController1_7>()?.gameObject;
        if (prisoner == null) return;

        // Hold UI가 최대 횟수 미만이고 현재 생성되어 있지 않으면 생성
        if (holdJudge.CanHold())
        {
            holdJudge.OnHoldStart(prisoner.transform);
        }
    }

    public void OnHoldSuccess()
    {
        holdCount++;

        PrisonerController1_7 prisoner =
            FindObjectOfType<PrisonerController1_7>();

        if (prisoner == null) return;

        if (holdCount >= maxHoldCount)
        {
            prisoner.DropToBasket(basket);

            Success();
            holdJudge.HideHoldUI();
        }
        else
        {
            Debug.Log($"Hold 성공! 현재 {holdCount} / {maxHoldCount}");
            holdJudge.OnHoldStart(prisoner.transform);
        }
    }


    //public void OnMiss()
    //{
    //    if (hasMissed) return;

    //    hasMissed = true;
    //    Debug.Log("미스 발생 -> 실패");

    //    Fail();
    //}
}
