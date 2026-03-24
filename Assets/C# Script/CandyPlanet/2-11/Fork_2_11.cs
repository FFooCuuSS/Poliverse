using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fork_2_11 : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeedX = 3f;
    public float leftX = -8f;
    public float rightX = 6f;

    private bool isDropping = false;

    private List<GameObject> skeweredMacarons = new List<GameObject>();
    public float stackSpacing = 0.3f;


    public float moveDistance = 2f;
    public float moveSpeed = 8f;

    private Vector3 startPos;
    private bool isMoving = false;

    private MacaroonPlate plate;

    private int macaronLayer;

    void Start()
    {
        transform.position = new Vector3(leftX, transform.position.y, transform.position.z);

        startPos = transform.position;
        plate = FindObjectOfType<MacaroonPlate>();

        macaronLayer = LayerMask.GetMask("Macaron");
    }

    void Update()
    {
        if (isDropping || isMoving) return;

        transform.position += Vector3.right * moveSpeedX * Time.deltaTime;

        if (transform.position.x >= rightX && !isDropping)
        {
            StartCoroutine(DropAllMacarons());
        }
    }

    public void GrabMacaron(GameObject macaron, Vector2 clickWorldPos)
    {
        if (isDropping || isMoving) return;
        StartCoroutine(GrabRoutine(macaron, clickWorldPos));
    }

    IEnumerator GrabRoutine(GameObject macaron, Vector2 clickPos)
    {
        isMoving = true;

        // 1. 포크 X 이동
        Vector3 targetPos = new Vector3(clickPos.x, transform.position.y, 0);
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        // 2. 아래로 내려가기
        Vector3 downPos = transform.position + Vector3.down * moveDistance;
        while (Vector3.Distance(transform.position, downPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, downPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        transform.position = downPos;

        // 내려간 후 OverlapCircle로 마카롱 감지
        Vector3 checkPos = transform.position + Vector3.down * moveDistance;
        float checkRadius = 0.7f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, checkRadius, macaronLayer);

        GameObject target = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            Macaron m = h.GetComponent<Macaron>();

            if (m.isStacked) continue; // 이미 잡은 마카롱 무시

            float dist = Vector2.Distance(checkPos, h.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                target = h.gameObject;
            }
        }

        if (target != null)
        {
            Debug.Log("잡은 마카롱: " + target.name);

            Macaron macScript = target.GetComponent<Macaron>();
            macScript.isStacked = true;

            target.layer = LayerMask.NameToLayer("Ignore Raycast");

            skeweredMacarons.Add(target);
            target.transform.SetParent(transform);

            int index = skeweredMacarons.Count - 1;
            target.transform.localPosition = new Vector3(0, -index * stackSpacing, 0);
        }
        else
        {
            Debug.Log("마카롱 없음");
        }

        // 3. 다시 올라가기
        Vector3 upPos = new Vector3(transform.position.x, startPos.y, transform.position.z);
        while (Vector3.Distance(transform.position, upPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, upPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        transform.position = upPos;

        isMoving = false;
    }

    IEnumerator DropAllMacarons()
    {
        isDropping = true;

        // 접시 위치로 이동
        Vector3 platePos = new Vector3(plate.transform.position.x, transform.position.y, 0);

        while (Vector3.Distance(transform.position, platePos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, platePos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        // 아래(먼저 꽂힌 것)부터 떨어뜨리기
        while (skeweredMacarons.Count > 0)
        {
            int lastIndex = skeweredMacarons.Count - 1;

            GameObject macaron = skeweredMacarons[lastIndex];
            skeweredMacarons.RemoveAt(lastIndex);

            macaron.transform.SetParent(null);

            Macaron m = macaron.GetComponent<Macaron>();
            plate.AddMacaron(m);

            yield return new WaitForSeconds(0.2f);
        }
    }

    // 포크 위치 시각화용
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position + Vector3.down * moveDistance, 0.5f);
    //}
}
