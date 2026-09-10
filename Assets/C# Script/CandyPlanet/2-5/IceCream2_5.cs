using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCream2_5 : MonoBehaviour
{
    private enum State
    {
        Fly,
        Drop,
        Stored
    }

    private State state = State.Fly;

    private Vector3 flyTarget;
    private Vector3 storeTarget;

    public float moveSpeed = 5f;
    public float fallSpeed = 5f;

    private bool landed = false;

    private Rigidbody2D rb;

    private List<IceCreamPipe> pipes;

    [Header("파이프 하이라이트 설정")]
    [Tooltip("파이프 중심 기준 하이라이트가 시작되는 X축 감지 거리")]
    public float highlightThreshold = 0.6f;

    [Tooltip("중심을 지난 후 추가로 유지될 최소 시간(초)")]
    public float minHighlightDuration = 0.15f;

    private IceCreamPipe currentHighlightedPipe;
    private Coroutine clearHighlightCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StartStoreMove()
    {
        state = State.Stored;
        ClearHighlightImmediate();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
        }
    }

    public void SetFlyTarget(Vector3 pos)
    {
        flyTarget = pos;
        state = State.Fly;
    }

    public void SetPipes(List<IceCreamPipe> pipeList)
    {
        pipes = pipeList;
    }

    public void SetStoreTarget(Vector3 pos)
    {
        storeTarget = new Vector3(pos.x, transform.position.y, transform.position.z);
    }

    public void Drop()
    {
        state = State.Drop;
        ClearHighlightImmediate();
    }

    void Update()
    {
        switch (state)
        {
            case State.Fly:
                MoveTo(flyTarget);
                CheckPipeProximity();
                break;

            case State.Drop:
                transform.position += Vector3.down * fallSpeed * Time.deltaTime;
                break;

            case State.Stored:
                MoveTo(storeTarget);
                break;
        }
    }

    private void CheckPipeProximity()
    {
        if (pipes == null || pipes.Count == 0) return;

        IceCreamPipe nearestPipe = null;
        float minDistance = float.MaxValue;

        foreach (var pipe in pipes)
        {
            if (pipe == null) continue;

            float distanceX = Mathf.Abs(transform.position.x - pipe.GetCenterX());
            if (distanceX <= highlightThreshold && distanceX < minDistance)
            {
                minDistance = distanceX;
                nearestPipe = pipe;
            }
        }

        if (currentHighlightedPipe != nearestPipe)
        {
            if (clearHighlightCoroutine != null)
            {
                StopCoroutine(clearHighlightCoroutine);
                clearHighlightCoroutine = null;
            }

            if (currentHighlightedPipe != null)
            {
                IceCreamPipe pipeToDisable = currentHighlightedPipe;
                clearHighlightCoroutine = StartCoroutine(DelayedClearHighlight(pipeToDisable, minHighlightDuration));
            }

            currentHighlightedPipe = nearestPipe;

            if (currentHighlightedPipe != null)
            {
                currentHighlightedPipe.SetHighlight(true);
            }
        }
    }

    private IEnumerator DelayedClearHighlight(IceCreamPipe pipe, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pipe != null)
        {
            pipe.SetHighlight(false);
        }
        clearHighlightCoroutine = null;
    }

    private void ClearHighlightImmediate()
    {
        if (clearHighlightCoroutine != null)
        {
            StopCoroutine(clearHighlightCoroutine);
            clearHighlightCoroutine = null;
        }

        if (currentHighlightedPipe != null)
        {
            currentHighlightedPipe.SetHighlight(false);
            currentHighlightedPipe = null;
        }
    }

    private void MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            if (state == State.Stored)
            {
                enabled = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Floor")) return;
        if (landed) return;

        landed = true;

        FindAnyObjectByType<IceCreamFloor>()
            .OnIceCreamLanded(transform);
    }

    private void OnDestroy()
    {
        ClearHighlightImmediate();
    }
}
