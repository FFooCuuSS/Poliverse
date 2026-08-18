using System.Collections;
using UnityEngine;

public class PracticePanelToggle :
    MonoBehaviour
{
    [SerializeField]
    private RectTransform panel;

    [SerializeField]
    private RectTransform arrowIcon;

    [SerializeField, Min(0f)]
    private float moveDuration = 0.25f;

    private Vector2 openedPosition;
    private Vector2 closedPosition;

    private bool isOpened = true;
    private bool initialized;

    private Coroutine moveCoroutine;

    private void EnsureInitialized()
    {
        if (initialized ||
            panel == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        openedPosition =
            panel.anchoredPosition;

        closedPosition =
            openedPosition +
            Vector2.left *
            panel.rect.width;

        initialized = true;
    }

    public void Toggle()
    {
        EnsureInitialized();

        if (panel == null)
            return;

        isOpened = !isOpened;

        Vector2 target =
            isOpened
                ? openedPosition
                : closedPosition;

        if (moveCoroutine != null)
        {
            StopCoroutine(
                moveCoroutine
            );
        }

        moveCoroutine =
            StartCoroutine(
                MovePanel(target)
            );

        UpdateArrow();
    }

    public void OpenImmediate()
    {
        EnsureInitialized();

        if (panel == null)
            return;

        if (moveCoroutine != null)
        {
            StopCoroutine(
                moveCoroutine
            );

            moveCoroutine = null;
        }

        isOpened = true;

        panel.anchoredPosition =
            openedPosition;

        UpdateArrow();
    }

    private IEnumerator MovePanel(
        Vector2 target)
    {
        Vector2 start =
            panel.anchoredPosition;

        float elapsed = 0f;

        if (moveDuration <= 0f)
        {
            panel.anchoredPosition =
                target;

            moveCoroutine = null;
            yield break;
        }

        while (elapsed <
               moveDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    moveDuration
                );

            panel.anchoredPosition =
                Vector2.Lerp(
                    start,
                    target,
                    t
                );

            yield return null;
        }

        panel.anchoredPosition =
            target;

        moveCoroutine = null;
    }

    private void UpdateArrow()
    {
        if (arrowIcon == null)
            return;

        arrowIcon.localEulerAngles =
            new Vector3(
                0f,
                0f,
                isOpened ? 0f : 180f
            );
    }
}