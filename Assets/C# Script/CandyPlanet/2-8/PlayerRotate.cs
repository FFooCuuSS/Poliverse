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

    public float CurrentAngle => targetAngle;

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
            bool isLeftClick = Input.mousePosition.x < centerX;

            // 게이트(입력 가능 여부/중복 방지)와 실제 회전 적용은 Minigame_2_8이 전담
            minigame_2_8.SubmitPlayerInput(isLeftClick);
        }
    }

    // Minigame_2_8이 게이트를 통과한 클릭에 대해서만 호출한다
    public void ApplyClickRotation(float direction)
    {
        targetAngle += direction * angle;
        targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);
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

        // 장애물 등에 의해 maxAngle을 넘어가면 실패
        if (Mathf.Abs(curAngle) > maxAngle)
        {
            // minigame_2_8.Failure();
        }
    }

    // 장애물이 떨어져서 강제로 각도를 변화시켜야 할 때 호출 (Show 시점 예고용)
    public void AddImpactAngle(float amount)
    {
        targetAngle += amount;
        targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);
    }
}