using UnityEngine;
using System.Collections;

public class Bawmquhen2_6 : MonoBehaviour, IPracticeDemoInput
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;

    [Header("이동")]
    [SerializeField] private float laneOffset = 3f;
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float stayTime = 0.15f;

    private Vector3 centerPos;

    private bool isMoving;
    private bool isDemoMode;

    private void Start()
    {
        centerPos = transform.position;

        isMoving = false;
        isDemoMode = false;
    }

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        Debug.Log(
            $"[Bawmquhen2_6] Demo Mode = {isDemoMode}"
        );

        if (isDemoMode)
        {
            StopAllCoroutines();

            isMoving = false;
        }
    }

    private void Update()
    {
        if (isDemoMode)
            return;

        if (isMoving)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x <
                Screen.width * 0.5f)
            {
                MoveToPlayerLane(0);
            }
            else
            {
                MoveToPlayerLane(2);
            }
        }
    }

    public void MoveToPlayerLane(int lane)
    {
        if (isDemoMode)
            return;

        float offset;

        switch (lane)
        {
            case 0:
                offset = -laneOffset;
                break;

            case 1:
                offset = 0f;
                break;

            case 2:
                offset = laneOffset;
                break;

            default:
                Debug.LogWarning(
                    $"[Bawmquhen2_6] 잘못된 Player Lane = {lane}"
                );

                return;
        }

        StartCoroutine(
            MoveAndReturn(offset)
        );
    }


    public void MoveToSafeLane(int safeLane)
    {
        if (!isDemoMode)
        {
            Debug.LogWarning(
                "[Bawmquhen2_6] Demo Mode가 아니어서 자동 이동을 실행하지 않습니다."
            );

            return;
        }

        if (isMoving)
        {
            StopAllCoroutines();

            isMoving = false;
        }

        float offset;

        switch (safeLane)
        {
            case 0:
                offset = -laneOffset;
                break;

            case 1:
                offset = 0f;
                break;

            case 2:
                offset = laneOffset;
                break;

            default:
                Debug.LogWarning(
                    $"[Bawmquhen2_6] 잘못된 Safe Lane = {safeLane}"
                );

                return;
        }

        Debug.Log(
            $"[Bawmquhen2_6] 안전 레인 {safeLane}으로 자동 이동"
        );

        StartCoroutine(
            MoveToLane(offset)
        );
    }

    private IEnumerator MoveToLane(float offset)
    {
        isMoving = true;

        PlayClickSound();

        Vector3 target =
            centerPos +
            Vector3.right * offset;

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
                    moveSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        transform.position = target;

        isMoving = false;
    }


    private IEnumerator MoveAndReturn(float offset)
    {
        isMoving = true;

        PlayClickSound();

        Vector3 target =
            centerPos +
            Vector3.right * offset;

        // 이동
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
                    moveSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        transform.position = target;

        // 잠시 유지
        yield return new WaitForSeconds(
            stayTime
        );

        // 중앙 복귀
        while (
            Vector3.Distance(
                transform.position,
                centerPos
            ) > 0.01f)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    centerPos,
                    moveSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        transform.position = centerPos;

        isMoving = false;
    }

    private void PlayClickSound()
    {
        if (clickSound != null &&
            GameRoot.Instance != null &&
            GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(
                clickSound
            );
        }
    }
}
