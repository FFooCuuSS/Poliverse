using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2_3 : MonoBehaviour
{
    [Header("스프라이트 리소스 설정")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite duckSprite;
    [SerializeField] private Sprite failSprite;

    [Header("크기 설정")]
    public Vector3 shrinkScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 normalScale = Vector3.one;
    public float restoreDelay = 1.0f;

    private bool isShrinking = false;
    private bool isFailed = false;

    [SerializeField] private Minigame_2_3 minigame;

    private bool currentFlipX = false;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
            spriteRenderer.flipX = false;
        }

        transform.localScale = normalScale;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (minigame != null && minigame.IsInputOpen)
            {
                minigame.OnPlayerInput();
                StartCoroutine(ShrinkAndRestore());
            }
        }
    }

    public void UpdateDirectionByHammerPosition(bool isHammerOnRight)
    {
        if (spriteRenderer == null || isFailed) return;

        if (!isHammerOnRight)
        {
            currentFlipX = false;
        }
        else
        {
            currentFlipX = true;
        }

        spriteRenderer.flipX = currentFlipX;
    }

    public void SetJudgementResult(bool isSuccess)
    {
        if (!isSuccess)
        {
            isFailed = true;

            if (failSprite != null)
                spriteRenderer.sprite = failSprite;

            StartCoroutine(RestoreAfterFail());
        }
    }

    IEnumerator ShrinkAndRestore()
    {
        if (isShrinking || isFailed)
            yield break;

        isShrinking = true;

        if (duckSprite != null)
            spriteRenderer.sprite = duckSprite;

        transform.localScale = shrinkScale;

        yield return new WaitForSeconds(restoreDelay);

        if (!isFailed)
        {
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;

            transform.localScale = normalScale;
            spriteRenderer.flipX = currentFlipX;
        }

        isShrinking = false;
    }

    IEnumerator RestoreAfterFail()
    {
        yield return new WaitForSeconds(restoreDelay);

        isFailed = false;
        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;

        transform.localScale = normalScale;
        spriteRenderer.flipX = currentFlipX;
    }
}