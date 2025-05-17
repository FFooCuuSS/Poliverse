using UnityEngine;
using System;

public abstract class MiniGameBase : MonoBehaviour
{
    public event Action OnSuccess;
    public event Action OnFail;

    public bool IsSuccess { get; protected set; }

    // ���ο��� override�� �� �ְ� ����
    protected virtual float TimerDuration => 10f;
    protected virtual string MinigameExplain => "�⺻ �̴ϰ��� ����";

    // �ܺο��� ���� �� �ֵ��� public getter ����
    public float GetTimerDuration => TimerDuration;
    public string GetMinigameExplain => MinigameExplain;

    public virtual void StartGame()
    {
        IsSuccess = false;
        Debug.Log($"{gameObject.name} ���� ����!");
        Debug.Log($"����: {MinigameExplain}");
        Debug.Log($"Ÿ�̸�: {TimerDuration}��");
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
        Debug.Log($"{gameObject.name} ����!");
        OnSuccess?.Invoke();
    }

    protected void Fail()
    {
        if (IsSuccess) return;
        IsSuccess = false;
        Debug.Log($"{gameObject.name} ����!");
        OnFail?.Invoke();
    }
}


/*
�ڽ� Ŭ���� ����

public class MiniGame_1_10 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "Space�� ���� �����ϼ���!";

    public override void StartGame()
    {
        base.StartGame(); // �θ𿡼� ����, Ÿ�̸� ��� �� ����

        // �߰� �ʱ�ȭ
        // ��: instructionText.text = MinigameExplain;
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
