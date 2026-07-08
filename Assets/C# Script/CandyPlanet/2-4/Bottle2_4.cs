using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Bottle2_4 : MonoBehaviour
{
    private Vector3 target;
    public float moveSpeed = 5f;
    private bool isFilled = false;

    public void SetTarget(Vector3 pos)
    {
        target = pos;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    public void FillBottle()
    {
        if (isFilled) return;
        isFilled = true;

        Debug.Log("보틀에 음료가 채워졌습니다!");
    }
}
