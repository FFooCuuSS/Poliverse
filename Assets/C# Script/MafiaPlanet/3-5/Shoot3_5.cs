using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot3_5 : MonoBehaviour
{
    public GameObject spyTracker;

    private Camera mainCam;
    public float followSpeed = 2f;
    public Vector2 moveLimitMin; // ī�޶� ���� ���� �ּ�
    public Vector2 moveLimitMax; // ī�޶� ���� ���� �ִ�

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.SetParent(this.transform); // ī�޶� �ڽ�����
        }
    }

    void Update()
    {
        // 1. ���콺 ��ġ�� ī�޶� �̵�
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = mainCam.transform.position.z;

        Vector3 targetPos = Vector3.Lerp(mainCam.transform.position, mouseWorldPos, followSpeed * Time.deltaTime);
        targetPos.x = Mathf.Clamp(targetPos.x, moveLimitMin.x, moveLimitMax.x);
        targetPos.y = Mathf.Clamp(targetPos.y, moveLimitMin.y, moveLimitMax.y);

        mainCam.transform.position = targetPos;

        // 2. ���콺 �Ʒ� ������Ʈ ����
        CheckMouseOverEnemy();
    }

    void CheckMouseOverEnemy()
    {
        Vector2 mouseWorld2D = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld2D, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            Debug.Log("���̴�");
        }
    }
}
