using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController1_1 : MonoBehaviour
{
    public Camera mainCam;
    public Transform targetObj;
    public float followSpeed = 0.5f;
    public Vector2 moveLimitMin; // 카메라 제한 범위 최소
    public Vector2 moveLimitMax; // 카메라 제한 범위 최대

    public bool isMoving = true;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!isMoving) return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = mainCam.transform.position.z;

        Vector3 targetPos = Vector3.Lerp(mainCam.transform.position, mouseWorldPos, followSpeed * Time.deltaTime);
        targetPos.x = Mathf.Clamp(targetPos.x, moveLimitMin.x, moveLimitMax.x);
        targetPos.y = Mathf.Clamp(targetPos.y, moveLimitMin.y, moveLimitMax.y);

        mainCam.transform.position = targetPos;
        targetObj.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0f);

        //CheckMouseOverEnemy();
    }

    void CheckMouseOverEnemy()
    {
        Vector2 mouseWorld2D = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld2D, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            Debug.Log("적이다");
        }
    }
}
