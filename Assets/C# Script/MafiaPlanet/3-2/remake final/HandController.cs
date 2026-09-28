using DG.Tweening;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("미니게임")]
    [SerializeField] private Minigame3_2remake minigame;


    [Header("오브젝트")]
    [SerializeField] private GameObject hand;
    [SerializeField] private GameObject grab;

    // Grab의 자식 Target
    [SerializeField] private Transform target;


    [Header("가로 이동 설정")]
    [SerializeField] private float startX = 12f;
    [SerializeField] private float moveY = 7f;

    [SerializeField] private float firstTargetX = -7f;
    [SerializeField] private float endX = -12f;

    // 12 -> -7까지 걸리는 시간
    [SerializeField] private float horizontalMoveTime = 2.4f;


    [Header("세로 이동 설정")]
    [SerializeField] private float downY = 4.4f;
    [SerializeField] private float upY = 9.5f;

    // 내려가는 시간 / 올라가는 시간
    [SerializeField] private float verticalMoveTime = 1.2f;


    // 현재 Hand와 충돌하고 있는 Bag
    private GameObject currentBag;

    // 실제로 잡은 Bag
    private GameObject grabbedBag;

    // 이미 클릭해서 동작이 시작됐는지
    private bool isActionStarted = false;

    // Bag을 실제로 잡았는지
    private bool hasBag = false;

    // 현재 실행 중인 가로 이동 Tween
    private Tween horizontalTween;


    private void Start()
    {
        // 시작 상태
        hand.SetActive(true);
        grab.SetActive(false);


        // Hand 시작 위치
        hand.transform.position =
            new Vector3(
                startX,
                moveY,
                hand.transform.position.z
            );


        // 가로 이동 시작
        StartHorizontalMove();
    }


    private void Update()
    {
        // Bag을 잡은 상태라면
        // Grab의 자식 Target 위치를 계속 따라간다.
        if (hasBag &&
            grabbedBag != null &&
            target != null)
        {
            grabbedBag.transform.position =
                target.position;
        }
    }


    /// <summary>
    /// Hand 가로 이동
    ///
    /// 12 -> -7 : 2.4초
    /// -7 -> -12 : 동일한 속도
    /// </summary>
    private void StartHorizontalMove()
    {
        // 12 -> -7 거리
        float firstDistance =
            Mathf.Abs(startX - firstTargetX);


        // 이동 속도 계산
        float speed =
            firstDistance / horizontalMoveTime;


        // -7 -> -12 거리
        float secondDistance =
            Mathf.Abs(firstTargetX - endX);


        // 동일 속도로 이동하기 위한 시간
        float secondMoveTime =
            secondDistance / speed;


        Sequence sequence =
            DOTween.Sequence();


        // 12 -> -7
        sequence.Append(
            hand.transform
                .DOMoveX(
                    firstTargetX,
                    horizontalMoveTime
                )
                .SetEase(Ease.Linear)
        );


        // -7 -> -12
        sequence.Append(
            hand.transform
                .DOMoveX(
                    endX,
                    secondMoveTime
                )
                .SetEase(Ease.Linear)
        );


        // 클릭하지 않고 -12까지 도착
        sequence.OnComplete(() =>
        {
            // 안전장치
            if (isActionStarted)
            {
                return;
            }


            Debug.Log(
                "[Hand] x = -12 도착"
            );


            // 미니게임에 Fail 전달
            if (minigame != null)
            {
                minigame.ReportHandTimeoutFail();
            }
        });


        horizontalTween = sequence;
    }


    /// <summary>
    /// 좌클릭했을 때 호출
    /// </summary>
    public void StartGrabAction()
    {
        // 이미 클릭했다면 중복 실행 방지
        if (isActionStarted)
        {
            return;
        }


        isActionStarted = true;


        Debug.Log(
            "[Hand] 클릭 - 하강 시작"
        );


        // 가로 이동 정지
        horizontalTween?.Kill();


        // 클릭 순간의 X 좌표
        float fixedX =
            hand.transform.position.x;


        // X 위치 고정
        hand.transform.position =
            new Vector3(
                fixedX,
                hand.transform.position.y,
                hand.transform.position.z
            );


        // 현재 X를 유지하면서
        // y = 4.4까지 1.2초 동안 이동
        hand.transform
            .DOMoveY(
                downY,
                verticalMoveTime
            )
            .SetEase(Ease.Linear)
            .OnComplete(OnReachedBottom);
    }


    /// <summary>
    /// Hand가 y = 4.4까지 내려왔을 때 호출
    /// </summary>
    private void OnReachedBottom()
    {
        Debug.Log(
            "[Hand] y = 4.4 도착"
        );


        // Bag과 충돌 중이면 무조건 잡는다.
        if (currentBag != null)
        {
            GrabBag(currentBag);
        }
        else
        {
            // Bag이 없으면 빈손으로 올라간다.
            MoveHandUp();
        }
    }


    /// <summary>
    /// Bag을 잡는다.
    /// </summary>
    private void GrabBag(GameObject bag)
    {
        hasBag = true;
        grabbedBag = bag;


        // Hand가 내려온 위치 저장
        Vector3 handPosition =
            hand.transform.position;


        // 기존 Hand 끄기
        hand.SetActive(false);


        // Grab 켜기
        grab.SetActive(true);


        // Grab을 Hand가 있던 위치에 배치
        grab.transform.position =
            handPosition;


        // Bag을 Grab의 Target 위치로 이동
        if (target != null)
        {
            grabbedBag.transform.position =
                target.position;
        }


        // Grab을 y = 9.5까지
        // 1.2초 동안 올린다.
        grab.transform
            .DOMoveY(
                upY,
                verticalMoveTime
            )
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Debug.Log(
                    "[Hand] Bag 잡기 완료"
                );
            });
    }


    /// <summary>
    /// Bag을 잡지 못한 경우
    /// Hand만 올라간다.
    /// </summary>
    private void MoveHandUp()
    {
        hand.transform
            .DOMoveY(
                upY,
                verticalMoveTime
            )
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Debug.Log(
                    "[Hand] 빈손 복귀 완료"
                );
            });
    }


    /// <summary>
    /// Hand가 Bag과 충돌했을 때 호출
    /// </summary>
    public void EnterBag(GameObject bag)
    {
        currentBag = bag;

        Debug.Log(
            "[Hand] Bag 충돌 시작 : " +
            bag.name
        );
    }


    /// <summary>
    /// Hand가 Bag에서 빠져나왔을 때 호출
    /// </summary>
    public void ExitBag(GameObject bag)
    {
        if (currentBag != bag)
        {
            return;
        }


        currentBag = null;

        Debug.Log(
            "[Hand] Bag 충돌 종료 : " +
            bag.name
        );
    }
}