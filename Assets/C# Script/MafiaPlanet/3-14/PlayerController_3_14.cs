using UnityEngine;
using DG.Tweening;

public class PlayerController_3_14 : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float stepX = 3f;

    [Header("Input")]
    [SerializeField] private bool useKeyboardInput = true;
    [SerializeField] private bool useMouseInput = true;

    private Manager_3_14 manager;
    private Vector3 roundStartPosition;
    private int currentStep;

    public void OnMinigameStart(Manager_3_14 mgr)
    {
        manager = mgr;
        roundStartPosition = transform.position;
        currentStep = 0;
    }

    private void Update()
    {
        if (useKeyboardInput)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
                PressMoveInput();
        }

        if (useMouseInput)
        {
            if (Input.GetMouseButtonDown(0))
                PressMoveInput();
        }
    }

    public void PressMoveInput()
    {
        manager?.RequestMoveInput();
    }

    public void MoveRightOneStep(float duration)
    {
        currentStep++;

        Vector3 target = roundStartPosition + new Vector3(stepX * currentStep, 0f, 0f);

        transform.DOKill();

        transform.DOMove(target, duration)
            .SetEase(Ease.OutQuad);
    }

    public void ResetToRoundStart(float duration)
    {
        currentStep = 0;

        transform.DOKill();

        if (duration <= 0f)
        {
            transform.position = roundStartPosition;
            return;
        }

        transform.DOMove(roundStartPosition, duration)
            .SetEase(Ease.OutQuad);
    }
}