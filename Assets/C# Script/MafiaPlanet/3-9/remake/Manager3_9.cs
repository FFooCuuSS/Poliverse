using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PatternSlot3_9
{
    [Header("해당 칸이 빛날 때 켜지는 오브젝트")]
    public GameObject onBox;

    [Header("해당 칸의 실제 클릭 대상")]
    public GameObject pattern;
}

public class Manager3_9 : MonoBehaviour
{
    [Header("1~6번 패턴")]
    public PatternSlot3_9[] slots = new PatternSlot3_9[6];

    [Header("미니게임 판정")]
    public Minigame_3_9_remake minigame;


    [Header("게임 시작 대기 시간")]
    public float startDelay = 2f;

    [Header("패턴 ON 시간")]
    public float patternOnTime = 0.5f;

    [Header("패턴 OFF 시간")]
    public float patternOffTime = 0.5f;

    [Header("패턴 표시 후 입력까지 대기 시간")]
    public float inputStartDelay = 2f;


    // 랜덤으로 만들어진 정답 순서
    // 예:
    // 3, 0, 5, 2, 4, 1
    //
    // 실제 번호:
    // 4 -> 1 -> 6 -> 3 -> 5 -> 2
    private List<int> answerPattern =
        new List<int>();


    // 현재 몇 번째 입력을 받고 있는지
    private int inputIndex = 0;


    // 현재 플레이어 입력 가능 여부
    private bool canInput = false;


    private void Start()
    {
        // 시작할 때 모든 onBox를 끈다.
        SetAllOnBoxes(false);

        // 게임 진행 시작
        StartCoroutine(GameRoutine());
    }


    /// <summary>
    /// 전체 3-9 게임 진행
    /// </summary>
    private IEnumerator GameRoutine()
    {
        Debug.Log("[3-9] 게임 시작");

        // 게임 시작 후 2초 대기
        yield return new WaitForSeconds(
            startDelay
        );


        // =========================
        // 랜덤 패턴 생성
        // =========================

        CreateRandomPattern();


        // =========================
        // 정답 패턴 보여주기
        // =========================

        yield return StartCoroutine(
            ShowPattern()
        );


        // =========================
        // 패턴 표시 후 2초 대기
        // =========================

        yield return new WaitForSeconds(
            inputStartDelay
        );


        // =========================
        // 입력 시작
        // =========================

        StartInput();


        // 6번의 입력이 끝날 때까지 대기
        while (canInput)
        {
            yield return null;
        }


        Debug.Log(
            "[3-9] 모든 입력 완료"
        );


        // =========================
        // 최종 결과 판정
        // =========================

        if (minigame != null)
        {
            minigame.FinishGame();
        }
        else
        {
            Debug.LogError(
                "[3-9] Minigame_3_9_remake가 연결되지 않음"
            );
        }
    }


    /// <summary>
    /// 1~6을 중복 없이 랜덤 순서로 만든다.
    /// </summary>
    private void CreateRandomPattern()
    {
        answerPattern.Clear();


        // 0~5 준비
        List<int> availableIndexes =
            new List<int>();


        for (int i = 0;
             i < slots.Length;
             i++)
        {
            availableIndexes.Add(i);
        }


        // 6개 전부 랜덤하게 뽑는다.
        while (availableIndexes.Count > 0)
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    availableIndexes.Count
                );


            int selectedSlot =
                availableIndexes[randomIndex];


            answerPattern.Add(
                selectedSlot
            );


            // 이미 뽑은 번호 제거
            availableIndexes.RemoveAt(
                randomIndex
            );
        }


        // 정답 확인용 로그
        string debugPattern = "";


        for (int i = 0;
             i < answerPattern.Count;
             i++)
        {
            debugPattern +=
                (answerPattern[i] + 1);


            if (i <
                answerPattern.Count - 1)
            {
                debugPattern += " -> ";
            }
        }


        Debug.Log(
            "[3-9] 정답 패턴 : " +
            debugPattern
        );
    }


    /// <summary>
    /// 생성된 6개의 패턴을 순서대로 보여준다.
    /// </summary>
    private IEnumerator ShowPattern()
    {
        // 패턴 표시 중에는 클릭 불가능
        canInput = false;


        for (int i = 0;
             i < answerPattern.Count;
             i++)
        {
            int slotIndex =
                answerPattern[i];


            // 해당 위치 켜기
            if (slots[slotIndex].onBox != null)
            {
                slots[slotIndex]
                    .onBox
                    .SetActive(true);
            }


            // ON 유지
            yield return new WaitForSeconds(
                patternOnTime
            );


            // 해당 위치 끄기
            if (slots[slotIndex].onBox != null)
            {
                slots[slotIndex]
                    .onBox
                    .SetActive(false);
            }


            // 다음 패턴까지 대기
            yield return new WaitForSeconds(
                patternOffTime
            );
        }


        Debug.Log(
            "[3-9] 패턴 표시 완료"
        );
    }


    /// <summary>
    /// 플레이어 입력 단계 시작
    /// </summary>
    private void StartInput()
    {
        // 첫 번째 입력부터 시작
        inputIndex = 0;


        // 입력 시작하면
        // onBox 1~6 전부 켜기
        SetAllOnBoxes(true);


        // 입력 허용
        canInput = true;


        Debug.Log(
            "[3-9] 입력 시작"
        );
    }


    /// <summary>
    /// PatternButton3_9에서 호출
    /// </summary>
    public void OnPatternClicked(int slotIndex)
    {
        if (!canInput)
        {
            Debug.Log("[3-9] 클릭했지만 현재 입력 불가");
            return;
        }

        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogError("[3-9] 잘못된 slotIndex : " + slotIndex);
            return;
        }

        if (inputIndex >= answerPattern.Count)
        {
            Debug.Log("[3-9] 이미 입력 완료");
            return;
        }


        // ==============================
        // 클릭한 위치 onBox 끄기
        // ==============================

        GameObject clickedOnBox =
            slots[slotIndex].onBox;

        if (clickedOnBox == null)
        {
            Debug.LogError(
                "[3-9] slot " +
                (slotIndex + 1) +
                "의 onBox가 Inspector에 연결되지 않음"
            );
        }
        else
        {
            Debug.Log(
                "[3-9] onBox OFF 시도 : " +
                clickedOnBox.name
            );

            clickedOnBox.SetActive(false);

            Debug.Log(
                "[3-9] OFF 이후 activeSelf = " +
                clickedOnBox.activeSelf
            );
        }


        // 현재 정답
        int correctIndex =
            answerPattern[inputIndex];


        // ==============================
        // 정답
        // ==============================

        if (slotIndex == correctIndex)
        {
            Debug.Log(
                "[3-9] " +
                (inputIndex + 1) +
                "번째 입력 성공 : " +
                (slotIndex + 1)
            );

            if (minigame != null)
            {
                minigame.ReportPatternSuccess();
            }
        }

        // ==============================
        // 오답
        // ==============================

        else
        {
            Debug.Log(
                "[3-9] " +
                (inputIndex + 1) +
                "번째 입력 실패" +
                " / 클릭 = " +
                (slotIndex + 1) +
                " / 정답 = " +
                (correctIndex + 1)
            );

            if (minigame != null)
            {
                minigame.ReportPatternFail();
            }
        }


        // 다음 입력으로 이동
        inputIndex++;


        // 6개 입력 완료
        if (inputIndex >= answerPattern.Count)
        {
            Debug.Log("[3-9] 6개 입력 완료");

            SetAllOnBoxes(false);

            canInput = false;
        }
    }


    /// <summary>
    /// 모든 onBox 켜기 / 끄기
    /// </summary>
    private void SetAllOnBoxes(
        bool active
    )
    {
        for (int i = 0;
             i < slots.Length;
             i++)
        {
            if (slots[i] == null)
            {
                continue;
            }


            if (slots[i].onBox != null)
            {
                slots[i]
                    .onBox
                    .SetActive(active);
            }
        }
    }
}