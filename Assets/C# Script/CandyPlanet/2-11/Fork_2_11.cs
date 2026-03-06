using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fork_2_11 : MonoBehaviour
{
    public float moveDistance = 2f;
    public float moveSpeed = 8f;

    private Vector3 startPos;
    private bool isMoving = false;

    private GameObject grabbedMacaron;

    private MacaroonPlate plate;

    void Start()
    {
        startPos = transform.position;
        plate = FindObjectOfType<MacaroonPlate>();
    }

    public void GrabMacaron(GameObject macaron)
    {
        if (isMoving) return;

        StartCoroutine(MoveFork(macaron));
    }

    IEnumerator MoveFork(GameObject macaron)
    {
        isMoving = true;

        // 마카롱 위치로 이동
        Vector3 targetPos = new Vector3(macaron.transform.position.x, startPos.y, startPos.z);

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        startPos = transform.position;

        Vector3 downPos = startPos + Vector3.down * moveDistance;

        // 내려가기
        while (Vector3.Distance(transform.position, downPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, downPos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        // 마카롱 집기
        grabbedMacaron = macaron;
        grabbedMacaron.transform.SetParent(transform);
        grabbedMacaron.transform.localPosition = new Vector3(0, 0.2f, 0);

        yield return new WaitForSeconds(0.1f);

        // 올라가기
        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, startPos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        // 접시 위치로 이동
        Vector3 platePos = new Vector3(plate.transform.position.x, transform.position.y, transform.position.z);

        while (Vector3.Distance(transform.position, platePos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, platePos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        // 마카롱 떨어뜨리기
        DropMacaron();

        isMoving = false;
    }

    void DropMacaron()
    {
        if (grabbedMacaron == null) return;

        grabbedMacaron.transform.SetParent(null);

        Macaron macaronScript = grabbedMacaron.GetComponent<Macaron>();
        plate.AddMacaron(macaronScript);

        grabbedMacaron = null;
    }
}
