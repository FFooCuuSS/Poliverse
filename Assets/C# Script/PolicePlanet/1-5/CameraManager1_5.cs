using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager1_5 : MonoBehaviour
{
    private Camera mainCam;
    public float followSpeed = 0.5f;
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
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = mainCam.transform.position.z;

        Vector3 targetPos = Vector3.Lerp(mainCam.transform.position, mouseWorldPos, followSpeed * Time.deltaTime);
        targetPos.x = Mathf.Clamp(targetPos.x, moveLimitMin.x, moveLimitMax.x);
        targetPos.y = Mathf.Clamp(targetPos.y, moveLimitMin.y, moveLimitMax.y);

        mainCam.transform.position = targetPos;


    }
}
