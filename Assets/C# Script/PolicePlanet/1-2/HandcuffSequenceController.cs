using UnityEngine;

public class HandcuffSequenceController : MonoBehaviour
{
    public static HandcuffSequenceController Instance;

    public HandAutoMove leftHand;
    public HandAutoMove rightHand;

    public HandcuffFitChecker leftCuff;
    public CircleCollider2D leftHandCollider;
    public CircleCollider2D rightHandCollider;

    public ChainGenerator chainGenerator;
    public DragAndDrop rightCuffDrag;

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
        if(Instance == null) Instance = this;
    }

    void Start()
    {
        rightCuffDrag.enabled = true; // 오른손은 처음부터 드래그 가능
        leftHand.StartMove();
        rightHandCollider.enabled = false;
    }

    void Update()
    {
        switch (curState)
        {
            case State.LeftMoving:
                if (leftHand.hasArrived)
                {
                    leftCuff.ForceSnapToHand(leftHandCollider);

                    leftHandCollider.enabled = false;
                    chainGenerator.isLeftCuffLocked = true; 

                    Debug.Log("왼손 수갑 장착 완료");
                    curState = State.LeftSnapped;
                    rightHandCollider.enabled = true ;
                }
                break;

            case State.LeftSnapped:
                rightHand.StartMove();
                curState = State.RightMoving;
                break;

            case State.RightMoving:
                if (rightHand.hasArrived)
                {
                    Debug.Log("오른손 도착 – 플레이어 조작");
                    curState = State.PlayerDrag;
                }
                break;
        }
    }
}
