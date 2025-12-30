using UnityEngine;

public class HandcuffSequenceController : MonoBehaviour
{
    public HandAutoMove leftHand;
    public HandAutoMove rightHand;

    public HandcuffFitChecker leftCuff;
    public CircleCollider2D leftHandCollider;

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
        rightCuffDrag.enabled = false; // 처음엔 조작 불가
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
                    rightCuffDrag.enabled = true; // 이제 유저 조작
                    state = State.PlayerDrag;
                }
                break;
        }
    }
}
