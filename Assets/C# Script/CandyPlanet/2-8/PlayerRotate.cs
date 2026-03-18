using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10f; // 회전 부드러움
    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private float angle = 15f;

    private float targetAngle = 0f; // 목표 각도 (0, 20, -20 중 하나)
    private Minigame_2_8 minigame_2_8;
    public GameObject stage_2_8;

    private void Start()
    {
        minigame_2_8 = stage_2_8.GetComponent<Minigame_2_8>();
        targetAngle = 0f;
    }

    private void Update()
    {
        HandleStepInput();
        ApplyRotation();
        CheckFailAngle();
    }

    private void HandleStepInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float centerX = Screen.width / 2f;

            if (Input.mousePosition.x < centerX) // 왼쪽 클릭
            {
                // 현재 오른쪽(-20)이면 중앙(0)으로, 중앙(0)이면 왼쪽(20)으로
                if (targetAngle <= 0) targetAngle += angle;
            }
            else // 오른쪽 클릭
            {
                // 현재 왼쪽(20)이면 중앙(0)으로, 중앙(0)이면 오른쪽(-20)으로
                if (targetAngle >= 0) targetAngle -= angle;
            }

            // [중요] 각도가 20, 0, -20 범위를 벗어나지 않도록 고정
            targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);
        }
    }

    private void ApplyRotation()
    {
        // targetAngle로 부드럽게 회전 모션
        Quaternion targetQuaternion = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, Time.deltaTime * rotateSpeed);
    }

    private void CheckFailAngle()
    {
        float curAngle = transform.eulerAngles.z;
        if (curAngle > 180f) curAngle -= 360f;

        // 장애물 등에 의해 maxAngle(45도)을 넘어가면 실패
        if (Mathf.Abs(curAngle) > maxAngle)
        {
            minigame_2_8.Failure();
        }
    }

    // 장애물이 떨어져서 강제로 각도를 변화시켜야 할 때 호출
    public void AddImpactAngle(float amount)
    {
        targetAngle += amount;
    }
}