using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_14 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "막으세요!";


    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    private bool ended;
    private bool inputOpen;          // Input 구간인지
    private bool awaitingJudge;      // 입력 후 판정 대기중(중복 입력 방지용)

    private const int MaxNodes = 5;

    private List<bool> nodeResults; // 각 노드 성공 여부 저장
    private int currentNode = 0;

    public int successCount = 0;

    public bool IsInputOpen => inputOpen;

    public override void StartGame()
    {

        nodeResults = new List<bool>();
        currentNode = 0;
        ended = false;
    }


    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        switch (action)
        {
            case "Show":
                float showInputWindow = 0.5f; // show 기준 ±0.5초 판정
                StartCoroutine(OpenInputWindowAroundShow(showInputWindow));

                float timeToHit = 1f; // 음식 이동 시간
                FindObjectOfType<FoodSpawn_2_14>()?.SpawnOneFood(timeToHit);
                break;
            case "Input":
                Debug.Log("Input → 판정 가능");
                inputOpen = true;
                StartCoroutine(CloseInputWindow(0.5f)); // 판정 시간
                break;
        }
    }

    IEnumerator OpenInputWindowAroundShow(float halfWindow)
    {
        // 0.5초 전 input 가능 (미리 true)
        yield return new WaitForSeconds(halfWindow);
        inputOpen = true;

        // 0.5초 후 판정 종료
        yield return new WaitForSeconds(halfWindow * 2);
        inputOpen = false;
    }

    public override void OnPlayerInput(string action = null)
    {
        if (ended) return;

        if (action == "Input")
        {
            rhythmManager?.ReceivePlayerInput("Input");
        }
    }

    IEnumerator CloseInputWindow(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputOpen = false;
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        /*
        if (ended) return;

        bool success = judgement == JudgementResult.Perfect || judgement == JudgementResult.Good;
        nodeResults.Add(success);

        Debug.Log($"노드 {currentNode + 1} 판정: {(success ? "성공" : "실패")}");

        currentNode++;

        if (currentNode >= MaxNodes)
        {
            ended = true;

            if (nodeResults.Contains(true))
            {
                Debug.Log("미니게임 성공! 1번이라도 성공함");
            }
            else
            {
                Debug.Log("미니게임 실패! 한 번도 성공 못함");
            }
        }
        */
        return;
    }
}
