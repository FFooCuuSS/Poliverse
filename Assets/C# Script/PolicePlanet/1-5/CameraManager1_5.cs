using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager1_5 : MonoBehaviour
{
    private Camera mainCam;
    public float followSpeed = 0.5f;
    public Vector2 moveLimitMin; // 카메라 제한 범위 최소
    public Vector2 moveLimitMax; // 카메라 제한 범위 최대

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.SetParent(this.transform); // 카메라를 자식으로
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
