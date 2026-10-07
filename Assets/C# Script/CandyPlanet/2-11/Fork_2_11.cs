using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fork_2_11 : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip successSound;
    [Header("효과음")]
    [SerializeField] private AudioClip movingSound;

    [Header("Move Settings")]
    public float moveSpeedX = 3f;

    public float leftX = -8f;
    public float rightX = 6f;

    private bool isDropping = false;

    private List<GameObject>
        skeweredMacarons =
        new List<GameObject>();

    public float stackSpacing = 0.3f;

    public float moveDistance = 2f;
    public float moveSpeed = 8f;

    private Vector3 startPos;

    private bool isMoving = false;

    private MacaroonPlate plate;
    private MacaroonSpawn spawner;

    private int macaronLayer;

    private Minigame_2_11 minigame;

    [SerializeField]
    public float stackStartOffsetY = -0.3f;

    private bool isDemoMode;


    private void Start()
    {
        transform.position =
            new Vector3(
                leftX,
                transform.position.y,
                transform.position.z
            );

        startPos =
            transform.position;

        plate =
            FindObjectOfType<MacaroonPlate>();

        spawner =
            FindObjectOfType<MacaroonSpawn>();

        minigame =
            FindObjectOfType<Minigame_2_11>();

        macaronLayer =
            LayerMask.GetMask("Macaron");

        isDemoMode = false;
    }



    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        Debug.Log(
            $"[Fork_2_11] Demo Mode = {isDemoMode}"
        );

        if (isDemoMode)
        {
            StopAllCoroutines();

            isMoving = false;
            isDropping = false;
        }
    }


    private void Update()
    {
        if (isDropping || isMoving)
        {
            return;
        }

        if (minigame != null && minigame.IsEnded)
        {
            return;
        }

        // [수정] 데모 모드여도 일반 플레이처럼 왼쪽에서 오른쪽으로 계속 이동하도록 변경
        transform.position += Vector3.right * moveSpeedX * Time.deltaTime;

        // 데모 모드일 때 마카롱과 위치가 닿으면 자동으로 집기 처리
        if (isDemoMode)
        {
            CheckAndGrabDemoMacarons();
        }

        if (transform.position.x >= rightX && !isDropping)
        {
            StartCoroutine(DropAllMacarons());
        }
    }



    public void GrabMacaron()
    {
        if (isDemoMode)
            return;

        if (isDropping ||
            isMoving)
        {
            return;
        }

        StartCoroutine(
            GrabRoutine()
        );
    }


    public IEnumerator GrabMacaronDemo()
    {
        if (!isDemoMode)
            yield break;

        if (isDropping ||
            isMoving)
        {
            yield break;
        }

        yield return StartCoroutine(
            GrabRoutine()
        );
    }


    public IEnumerator MoveToMacaronX(
        float targetX)
    {
        if (!isDemoMode)
            yield break;

        if (isDropping)
            yield break;

        isMoving = true;

        Debug.Log(
            $"[Fork_2_11 Demo] " +
            $"포크 이동: {transform.position.x} → {targetX}"
        );

        if (movingSound != null &&
            GameRoot.Instance != null &&
            GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(
                movingSound
            );
        }

        Vector3 target =
            new Vector3(
                targetX,
                transform.position.y,
                transform.position.z
            );

        while (
            Vector3.Distance(
                transform.position,
                target
            ) > 0.01f)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeedX *
                    Time.deltaTime
                );

            yield return null;
        }

        transform.position =
            target;

        isMoving = false;
    }


    private IEnumerator GrabRoutine()
    {
        isMoving = true;

        if (movingSound != null &&
            GameRoot.Instance != null &&
            GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(
                movingSound
            );
        }


        Vector3 downPos =
            transform.position +
            Vector3.down *
            moveDistance;

        while (
            Vector3.Distance(
                transform.position,
                downPos
            ) > 0.05f)
        {
            transform.position =
                Vector3.Lerp(
                    transform.position,
                    downPos,
                    Time.deltaTime *
                    moveSpeed
                );

            yield return null;
        }

        transform.position =
            downPos;


        Vector3 checkPos =
            transform.position +
            Vector3.down *
            0.5f;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                checkPos,
                0.7f,
                macaronLayer
            );

        GameObject target = null;

        float minDist =
            float.MaxValue;

        foreach (var h in hits)
        {
            Macaron m =
                h.GetComponent<Macaron>();

            if (m == null)
                continue;

            if (m.isStacked)
                continue;

            float dist =
                Vector2.Distance(
                    checkPos,
                    h.transform.position
                );

            if (dist < minDist)
            {
                minDist = dist;
                target = h.gameObject;
            }
        }


        if (target != null)
        {
            Debug.Log(
                "[Fork_2_11] 마카롱 획득"
            );

            if (successSound != null &&
                GameRoot.Instance != null &&
                GameRoot.Instance.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(
                    successSound
                );
            }

            Macaron mac =
                target.GetComponent<Macaron>();

            mac.isStacked = true;

            target.transform.SetParent(
                transform
            );

            skeweredMacarons.Insert(
                0,
                target
            );


            for (int i = 0;
                 i < skeweredMacarons.Count;
                 i++)
            {
                skeweredMacarons[i]
                    .transform.localPosition =
                    new Vector3(
                        0,
                        stackStartOffsetY +
                        (i * stackSpacing),
                        0
                    );

                SpriteRenderer sr =
                    skeweredMacarons[i]
                        .GetComponent<SpriteRenderer>();

                if (sr != null)
                {
                    sr.sortingLayerName =
                        "Macaron";

                    sr.sortingOrder =
                        50 + i;
                }
            }

            // 직접 플레이에서만 성공 판정
            if (!isDemoMode)
            {
                minigame?.MacaronSuccess();
            }
        }
        else
        {
            Debug.Log(
                "[Fork_2_11] 포크 아래에 마카롱이 없습니다."
            );
        }


        Vector3 upPos =
            new Vector3(
                transform.position.x,
                startPos.y,
                transform.position.z
            );

        while (
            Vector3.Distance(
                transform.position,
                upPos
            ) > 0.05f)
        {
            transform.position =
                Vector3.Lerp(
                    transform.position,
                    upPos,
                    Time.deltaTime *
                    moveSpeed
                );

            yield return null;
        }

        transform.position =
            upPos;

        isMoving = false;
    }


    private IEnumerator DropAllMacarons()
    {
        isDropping = true;

        if (movingSound != null &&
            GameRoot.Instance != null &&
            GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(
                movingSound
            );
        }

        Vector3 platePos =
            new Vector3(
                plate.transform.position.x,
                transform.position.y,
                0
            );

        while (
            Vector3.Distance(
                transform.position,
                platePos
            ) > 0.05f)
        {
            transform.position =
                Vector3.Lerp(
                    transform.position,
                    platePos,
                    Time.deltaTime *
                    moveSpeed
                );

            yield return null;
        }

        while (
            skeweredMacarons.Count > 0)
        {
            GameObject macaron =
                skeweredMacarons[0];

            skeweredMacarons.RemoveAt(0);

            macaron.transform.SetParent(
                null
            );

            Macaron m =
                macaron.GetComponent<Macaron>();

            plate.AddMacaron(m);

            yield return new WaitForSeconds(
                0.2f
            );
        }

        // 못 집은 마카롱 제거
        spawner?.ClearUncollectedMacarons();

        // 접시 초기화
        plate?.ClearPlate();

        // 포크 초기화
        transform.position =
            startPos;

        isDropping = false;

        // 다음 패턴
        minigame?.SpawnNextRound();
    }

    private void CheckAndGrabDemoMacarons()
    {
        if (spawner == null) return;

        List<GameObject> macarons = spawner.GetSpawnedMacarons();
        if (macarons == null) return;

        float forkX = transform.position.x;

        foreach (var macaron in macarons)
        {
            if (macaron == null) continue;

            Macaron mac = macaron.GetComponent<Macaron>();
            if (mac == null || mac.isStacked) continue;

            // 포크의 X 좌표가 마카롱의 X 좌표와 매우 가까워졌을 때(지나칠 때) 자동으로 집기 실행
            if (Mathf.Abs(forkX - macaron.transform.position.x) <= 0.15f)
            {
                StartCoroutine(GrabRoutineForDemo(macaron));
                break;
            }
        }
    }

    private IEnumerator GrabRoutineForDemo(GameObject target)
    {
        isMoving = true;

        if (movingSound != null && GameRoot.Instance?.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(movingSound);
        }

        Vector3 downPos = transform.position + Vector3.down * moveDistance;

        // 아래로 내려가기
        while (Vector3.Distance(transform.position, downPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, downPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        transform.position = downPos;

        // 마카롱 획득 처리
        if (target != null)
        {
            if (successSound != null && GameRoot.Instance?.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(successSound);
            }

            Macaron mac = target.GetComponent<Macaron>();
            if (mac != null && !mac.isStacked)
            {
                mac.isStacked = true;
                target.transform.SetParent(transform);
                skeweredMacarons.Insert(0, target);

                // 포크에 꽂힌 마카롱들 위치/정렬 갱신
                for (int i = 0; i < skeweredMacarons.Count; i++)
                {
                    skeweredMacarons[i].transform.localPosition = new Vector3(
                        0,
                        stackStartOffsetY + (i * stackSpacing),
                        0
                    );

                    SpriteRenderer sr = skeweredMacarons[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sortingLayerName = "Macaron";
                        sr.sortingOrder = 50 + i;
                    }
                }
            }
        }

        // 원래 위쪽 위치로 복귀
        Vector3 upPos = new Vector3(transform.position.x, startPos.y, transform.position.z);
        while (Vector3.Distance(transform.position, upPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, upPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        transform.position = upPos;

        isMoving = false;
    }
}
