using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScrollController : MonoBehaviour
{
    [Header("패널 위치들")]
    public Transform[] panels;

    [Header("카메라 이동 속도")]
    public float smoothSpeed = 5f;

    [Header("Panel 2에서 자동 이동까지 대기 시간")]
    public float autoMoveDelay = 3f;      // 인스펙터에서 조절 가능

    private Vector3 targetPosition;
    private int currentPanelIndex = 0;
    private bool isAutoMoving = false;

    void Start()
    {
        // 시작 위치 설정
        if (panels.Length > 0)
        {
            targetPosition = transform.position;
        }
    }

    void Update()
    {
        // 카메라를 부드럽게 목표 위치로 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }

    // 외부(버튼)에서 호출하는 이동 함수
    public void MoveToPanel(int index)
    {
        if (index >= 0 && index < panels.Length)
        {
            currentPanelIndex = index;
            Vector3 panelPos = panels[index].position;
            targetPosition = new Vector3(transform.position.x, panelPos.y, transform.position.z);

            // Panel 2에 진입하면 자동 이동 코루틴 시작
            if (index == 1 && !isAutoMoving)
            {
                StartCoroutine(AutoMoveToNextPanelAfterDelay());
            }
        }
    }

    private IEnumerator AutoMoveToNextPanelAfterDelay()
    {
        isAutoMoving = true;
        yield return new WaitForSeconds(autoMoveDelay);

        // 현재가 여전히 Panel 2면 → Panel 3로 이동
        if (currentPanelIndex == 1 && panels.Length > 2)
        {
            MoveToPanel(2);
        }
    }
}
