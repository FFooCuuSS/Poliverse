using UnityEngine;
using System;
using System.Collections;

public abstract class MiniGameBase : MonoBehaviour
{
    public event Action OnSuccess;
    public event Action OnFail;

    public bool IsSuccess { get; protected set; }
    public bool IsInputLocked { get; protected set; } = false;

    // 내부에서 override할 수 있게 유지
    protected virtual float TimerDuration => 10f;
    protected virtual string MinigameExplain => "기본 미니게임 설명";

    // 외부에서 읽을 수 있도록 public getter 제공
    public float GetTimerDuration => TimerDuration;
    public string GetMinigameExplain => MinigameExplain;

    public virtual void StartGame()
    {
        IsSuccess = false;
        IsInputLocked = false;
        Debug.Log($"{gameObject.name} 게임 시작!");
        Debug.Log($"설명: {MinigameExplain}");
        Debug.Log($"타이머: {TimerDuration}초");
    }

    public virtual void ResetGame()
    {
        IsSuccess = false;
        OnSuccess = null;
        OnFail = null;
    }

    protected void Success()
    {
        if (IsSuccess) return;
        IsSuccess = true;
        Debug.Log($"{gameObject.name} 성공!");
        OnSuccess?.Invoke();
        StartCoroutine(LockInputTemporarily(3f));
    }

    protected void Fail()
    {
        if (IsSuccess) return;
        IsSuccess = false;
        Debug.Log($"{gameObject.name} 실패!");
        OnFail?.Invoke();
        StartCoroutine(LockInputTemporarily(3f));
    }

    protected IEnumerator LockInputTemporarily(float duration)
    {
        IsInputLocked = true;
        yield return new WaitForSeconds(duration);
        IsInputLocked = false;
    }
}


/*
자식 클래스 예시

public class MiniGame_1_10 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "Space를 눌러 점프하세요!";

    public override void StartGame()
    {
        base.StartGame(); // 부모에서 설명, 타이머 출력 등 실행

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Success();
        }
    }
}
*/
