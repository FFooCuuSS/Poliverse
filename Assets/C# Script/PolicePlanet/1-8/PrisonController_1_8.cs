using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonController_1_8 : MonoBehaviour
{
    public float downY = -3f;
    public float upY = 0f;
    public float moveSpeed = 8f;

    private bool isMoving = false;
    public bool IsActive { get; private set; }

    public void ActivatePrison()
    {
        if (!isMoving)
        {
            StartCoroutine(MovePrison());
        }
    }

    IEnumerator MovePrison()
    {
        isMoving = true;

        // 내려가기
        IsActive = true;

        while (transform.position.y > downY)
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.15f);

        // 올라가기
        IsActive = false;

        while (transform.position.y < upY)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }
}
