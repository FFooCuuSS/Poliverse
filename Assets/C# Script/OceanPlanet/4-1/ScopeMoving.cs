using System.Collections;
using UnityEngine;

public class ScopeMoving : MonoBehaviour
{
    [SerializeField]
    private float moveDistance = 1f;

    [SerializeField]
    private float moveInterval = 0.5f;

    private Coroutine movingCoroutine;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.localPosition;
    }

    public void StartMoving()
    {
        if (movingCoroutine != null)
        {
            return;
        }

        movingCoroutine = StartCoroutine(MoveCoroutine());
    }

    public void StopMoving()
    {
        if (movingCoroutine == null)
        {
            return;
        }

        StopCoroutine(movingCoroutine);
        movingCoroutine = null;
    }

    private IEnumerator MoveCoroutine()
    {
        bool moveRight = true;

        while (true)
        {
            yield return new WaitForSeconds(moveInterval);

            Vector3 targetPosition = startPosition;

            if (moveRight)
            {
                targetPosition.x += moveDistance;
            }
            else
            {
                targetPosition.x -= moveDistance;
            }

            transform.localPosition = targetPosition;
            moveRight = !moveRight;
        }
    }

    private void OnDisable()
    {
        if (movingCoroutine != null)
        {
            StopCoroutine(movingCoroutine);
            movingCoroutine = null;
        }

        transform.localPosition = startPosition;
    }
}
