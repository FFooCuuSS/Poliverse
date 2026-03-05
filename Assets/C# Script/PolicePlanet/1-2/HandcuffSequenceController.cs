using System.Collections;
using UnityEngine;

public class HandcuffSequenceController : MonoBehaviour
{
    public static HandcuffSequenceController Instance;

    public HandAutoMove leftHand;
    public HandAutoMove rightHand;

    public HandcuffFitChecker leftCuff; // 왼손 자동 장착 수갑
    public CircleCollider2D leftHandCollider;
    public CircleCollider2D rightHandCollider;

    public ChainGenerator chainGenerator;
    public DragAndDrop rightCuffDrag;

    [Header("Debug")]
    public bool autoStartDebug = false; // ✅ 원하면 Start에서 자동 시작

    public enum State
    {
        LeftMoving,
        LeftSnapped,
        RightMoving,
        PlayerDrag
    }

    public State curState { get; private set; } = State.LeftMoving;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (autoStartDebug)
        {
            ResetForRound();
            StartLeftMove(3.5f);
        }
        else
        {
            // ✅ 자동 시작 제거: Show 이벤트가 시작시킴
            ResetForRound();
        }
    }

    // ✅ ADD: Show마다 호출할 리셋
    public void ResetForRound()
    {
        curState = State.LeftMoving;

        if (leftHand != null) leftHand.ResetToStart();
        if (rightHand != null) rightHand.ResetToStart();

        if (leftHandCollider != null) leftHandCollider.enabled = true;
        if (rightHandCollider != null) rightHandCollider.enabled = false;

        if (rightCuffDrag != null) rightCuffDrag.enabled = true;

        if (chainGenerator != null) chainGenerator.isLeftCuffLocked = false;

        // 왼손 수갑(자동 장착)이 활성이라면 초기 상태로(필요 시)
        if (leftCuff != null)
        {
            leftCuff.gameObject.SetActive(true);
        }
    }

    // ✅ ADD: 왼손 내려오기 시작
    public void StartLeftMove(float totalTime)
    {
        if (leftHand == null) return;
        curState = State.LeftMoving;

        leftHand.StartMove(totalTime);
    }

    // ✅ ADD: 오른손 내려오기 시작
    public void StartRightMove(float totalTime)
    {
        if (rightHand == null) return;

        // 오른손이 움직이기 전에 왼손 장착 처리 흐름이 필요하면 Update에서 처리되게 둔다
        // 여기서는 상태를 오른손 이동으로 전환
        if (rightHandCollider != null) rightHandCollider.enabled = false;
        curState = State.RightMoving;

        rightHand.StartMove(totalTime);
    }

    // ✅ ADD: Input 실패/성공 후 빠른 디스폰
    public void DespawnAll(float fadeSeconds = 0.05f)
    {
        StartCoroutine(DespawnCo(fadeSeconds));
    }

    private IEnumerator DespawnCo(float fadeSeconds)
    {
        yield return new WaitForSeconds(fadeSeconds);

        if (leftHand != null) leftHand.gameObject.SetActive(false);
        if (rightHand != null) rightHand.gameObject.SetActive(false);
        if (leftCuff != null) leftCuff.gameObject.SetActive(false);

        // 오른손 수갑 오브젝트는 FitChecker가 붙은 애일 텐데,
        // 씬에 여러 개면 FitChecker 쪽에서 같이 꺼주는 게 더 안전함(아래 FitChecker 수정 참고)
        var all = FindObjectsOfType<HandcuffFitChecker>();
        foreach (var c in all) c.gameObject.SetActive(false);
    }

    void Update()
    {
        switch (curState)
        {
            case State.LeftMoving:
                if (leftHand != null && leftHand.hasArrived)
                {
                    // 왼손 수갑 자동 장착
                    if (leftCuff != null && leftHandCollider != null)
                        leftCuff.ForceSnapToHand(leftHandCollider);

                    if (leftHandCollider != null) leftHandCollider.enabled = false;
                    if (chainGenerator != null) chainGenerator.isLeftCuffLocked = true;

                    curState = State.LeftSnapped;

                    // 오른손 콜라이더는 "도착 후" 활성화해도 되고,
                    // 지금처럼 다음 단계에서 켜도 됨. 여기선 다음 단계로 넘김.
                }
                break;

            case State.LeftSnapped:
                // 오른손 이동은 Minigame_1_2가 Show2에서 StartRightMove로 시작한다.
                // 따라서 여기서는 아무것도 안 함.
                break;

            case State.RightMoving:
                if (rightHand != null && rightHand.hasArrived)
                {
                    // 오른손 도착 → 드래그 허용 상태 진입
                    if (rightHandCollider != null) rightHandCollider.enabled = true;
                    curState = State.PlayerDrag;
                }
                break;

            case State.PlayerDrag:
                break;
        }
    }
}