using UnityEngine;

public class HandcuffSequenceController : MonoBehaviour
{
    public HandAutoMove leftHand;
    public HandAutoMove rightHand;

    public HandcuffFitChecker leftCuff;
    public CircleCollider2D leftHandCollider;

    public ChainGenerator chainGenerator;
    public DragAndDrop rightCuffDrag;

    private enum State
    {
        LeftMoving,
        LeftSnapped,
        RightMoving,
        PlayerDrag
    }

    private State state = State.LeftMoving;

    void Start()
    {
        rightCuffDrag.enabled = true; // 오른손은 처음부터 드래그 가능
        leftHand.StartMove();
    }

    void Update()
    {
        switch (state)
        {
            case State.LeftMoving:
                if (leftHand.hasArrived)
                {
                    leftCuff.ForceSnapToHand(leftHandCollider);

                    leftHandCollider.enabled = false;
                    chainGenerator.isLeftCuffLocked = true; // 🔒 핵심

                    Debug.Log("왼손 수갑 장착 완료");
                    state = State.LeftSnapped;
                }
                break;

            case State.LeftSnapped:
                rightHand.StartMove();
                state = State.RightMoving;
                break;

            case State.RightMoving:
                if (rightHand.hasArrived)
                {
                    Debug.Log("오른손 도착 – 플레이어 조작");
                    state = State.PlayerDrag;
                }
                break;
        }
    }
}
