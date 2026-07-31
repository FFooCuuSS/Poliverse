using System;
using UnityEngine;

public class FoodRotate : MonoBehaviour
{
    [Header("회전 속도 (양수 = 시계 방향, 음수 = 반시계)")]
    public float rotateSpeed = 60f;

    [Header("속도 랜덤 범위 적용 여부")]
    public bool useRandomSpeed = false;
    public float minSpeed = 40f;
    public float maxSpeed = 100f;

    // 중앙 도착 후 "판정용 한 바퀴"가 끝났을 때 호출됨
    public event Action OnOneRevolutionComplete;

    private bool isJudging = false;
    private float rotatedAmount = 0f;

    public bool IsJudging => isJudging;

    private void Start()
    {
        if (useRandomSpeed)
        {
            rotateSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed) * (UnityEngine.Random.value > 0.5f ? 1 : -1);
        }
    }

    private void Update()
    {
        float delta = rotateSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward * delta);

        if (isJudging)
        {
            rotatedAmount += Mathf.Abs(delta);
            if (rotatedAmount >= 360f)
            {
                isJudging = false;
                OnOneRevolutionComplete?.Invoke();
            }
        }
    }

    // 중앙(spawnPoint) 도착 시 FoodManager가 호출: 이 시점부터 딱 한 바퀴만 추적
    public void BeginJudgementRotation()
    {
        rotatedAmount = 0f;
        isJudging = true;
    }
}