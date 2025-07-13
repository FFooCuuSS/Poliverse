using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScrollController : MonoBehaviour
{
    [Header("�г� ��ġ��")]
    public Transform[] panels;

    [Header("ī�޶� �̵� �ӵ�")]
    public float smoothSpeed = 5f;

    [Header("Panel 2���� �ڵ� �̵����� ��� �ð�")]
    public float autoMoveDelay = 3f;      // �ν����Ϳ��� ���� ����

    private Vector3 targetPosition;
    private int currentPanelIndex = 0;
    private bool isAutoMoving = false;

    void Start()
    {
        // ���� ��ġ ����
        if (panels.Length > 0)
        {
            targetPosition = transform.position;
        }
    }

    void Update()
    {
        // ī�޶� �ε巴�� ��ǥ ��ġ�� �̵�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }

    // �ܺ�(��ư)���� ȣ���ϴ� �̵� �Լ�
    public void MoveToPanel(int index)
    {
        if (index >= 0 && index < panels.Length)
        {
            currentPanelIndex = index;
            Vector3 panelPos = panels[index].position;
            targetPosition = new Vector3(transform.position.x, panelPos.y, transform.position.z);

            // Panel 2�� �����ϸ� �ڵ� �̵� �ڷ�ƾ ����
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

        // ���簡 ������ Panel 2�� �� Panel 3�� �̵�
        if (currentPanelIndex == 1 && panels.Length > 2)
        {
            MoveToPanel(2);
        }
    }
}
