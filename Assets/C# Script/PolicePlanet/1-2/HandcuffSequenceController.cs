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
        rightCuffDrag.enabled = false; // ½ÃÀÛÀº ¸·¾ÆµÒ
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

                    rightCuffDrag.enabled = true;
                    Debug.Log("¿Þ¼Õ ¼ö°©Ã¤¿ò, ¼ö°© µå·¡±× °¡´É");

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
                    state = State.PlayerDrag;
                    Debug.Log("¿À¸¥¼Õ µµÂø");
                }
                break;
        }
    }
}
