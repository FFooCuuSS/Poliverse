using UnityEngine;
using System.Collections;

public class Bawmquhen2_6 : MonoBehaviour
{
    [SerializeField] private float laneOffset = 3f;
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float stayTime = 0.15f;

    private Vector3 centerPos;
    private bool isMoving;

    void Start()
    {
        centerPos = transform.position;
    }

    void Update()
    {
        if (isMoving)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x < Screen.width * 0.5f)
            {
                StartCoroutine(MoveAndReturn(-laneOffset));
            }
            else
            {
                StartCoroutine(MoveAndReturn(laneOffset));
            }
        }
    }

    IEnumerator MoveAndReturn(float offset)
    {
        isMoving = true;

        Vector3 target = centerPos + Vector3.right * offset;

        // 이동
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime);

            yield return null;
        }

        // 잠깐 유지
        yield return new WaitForSeconds(stayTime);

        // 가운데 복귀
        while (Vector3.Distance(transform.position, centerPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                centerPos,
                moveSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = centerPos;
        isMoving = false;
    }
}
