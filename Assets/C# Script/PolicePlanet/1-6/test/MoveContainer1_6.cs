using UnityEngine;

public class MoveContainer1_6 : MonoBehaviour
{
    [Header("Container Move")]
    public float startX = 12f;

    private float targetX;
    private float fixedY;
    private float moveDuration;

    private float elapsedTime;
    private bool isMoving;

    public void InitMove(float startXValue, float targetXValue, float yValue, float duration)
    {
        startX = startXValue;
        targetX = targetXValue;
        fixedY = yValue;
        moveDuration = duration;

        transform.position = new Vector3(startX, fixedY, 0f);

        elapsedTime = 0f;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        elapsedTime += Time.deltaTime;

        if (moveDuration <= 0f)
        {
            transform.position = new Vector3(targetX, fixedY, 0f);
            isMoving = false;
            return;
        }

        float t = elapsedTime / moveDuration;

        if (t >= 1f)
        {
            t = 1f;
            isMoving = false;
        }

        float newX = Mathf.Lerp(startX, targetX, t);
        transform.position = new Vector3(newX, fixedY, 0f);
    }

    public float GetTargetX()
    {
        return targetX;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool HasArrived()
    {
        return !isMoving;
    }
}