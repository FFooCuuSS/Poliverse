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

    private Minigame_2_11 minigame;

    [SerializeField] public float stackStartOffsetY = -0.3f;

    void Start()
    {
        transform.position =
            new Vector3(
                leftX,
                transform.position.y,
                transform.position.z
            );

        startPos = transform.position;

        plate = FindObjectOfType<MacaroonPlate>();

        minigame = FindObjectOfType<Minigame_2_11>();

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
    public void GrabMacaron()
    {
        if (isDropping || isMoving)
            return;

        StartCoroutine(GrabRoutine());
    }

    IEnumerator GrabRoutine()
    {
        isMoving = true;

        // 현재 위치에서 바로 아래로 이동
        Vector3 downPos =
            transform.position + Vector3.down * moveDistance;


        while (Vector3.Distance(transform.position, downPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                downPos,
                Time.deltaTime * moveSpeed
            );

            yield return null;
        }

        transform.position = downPos;

        // 포크 아래 마카롱 체크
        Vector3 checkPos = transform.position + Vector3.down * 0.5f;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
            checkPos,
            0.7f,
            macaronLayer
        );

        GameObject target = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            Macaron m = h.GetComponent<Macaron>();

            if (m == null)
                continue;

            if (m.isStacked)
                continue;

            float dist = Vector2.Distance(checkPos, h.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                target = h.gameObject;
            }
        }

        if (target != null)
        {
            Debug.Log("마카롱 획득");

            Macaron mac = target.GetComponent<Macaron>();

            mac.isStacked = true;

            target.transform.SetParent(transform);

            skeweredMacarons.Insert(0, target);

            for (int i = 0; i < skeweredMacarons.Count; i++)
            {
                skeweredMacarons[i].transform.localPosition =
                    new Vector3(
                        0,
                        stackStartOffsetY + i * stackSpacing,
                        0
                    );
            }

            minigame.MacaronSuccess();
        }
        else
        {
            Debug.Log("마카롱 실패");

            minigame.MacaronFail();
        }

        // 다시 원래 높이로 올라가기
        Vector3 upPos =
            new Vector3(
                transform.position.x,
                startPos.y,
                transform.position.z
            );

        while (Vector3.Distance(transform.position, upPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                upPos,
                Time.deltaTime * moveSpeed
            );

            yield return null;
        }

        transform.position = upPos;

        isMoving = false;
    }

    IEnumerator DropAllMacarons()
    {
        isDropping = true;

        Vector3 platePos = new Vector3(
            plate.transform.position.x,
            transform.position.y,
            0
        );

        while (Vector3.Distance(transform.position, platePos) > 0.05f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                platePos,
                Time.deltaTime * moveSpeed
            );

            yield return null;
        }

        // 아래에 있는 마카롱부터 떨어뜨리기
        while (skeweredMacarons.Count > 0)
        {
            // 현재 맨 아래 마카롱
            GameObject macaron = skeweredMacarons[0];

            skeweredMacarons.RemoveAt(0);

            macaron.transform.SetParent(null);

            Macaron m = macaron.GetComponent<Macaron>();

            plate.AddMacaron(m);

            yield return new WaitForSeconds(0.2f);
        }
    }
}
