using System;
using System.Collections;
using UnityEngine;

public class Icicle : MonoBehaviour
{
    public static event Action OnMoveAllowed;
    public static event Action OnMoveBlocked;

    [Header("Sprite")]
    [SerializeField] private Sprite fallingSprite;

    [Header("Warning Move Distance")]
    [SerializeField] private float warningMoveDistance = 0.5f;

    private SpriteRenderer sr;
    private Rigidbody2D rb;

    public static event Action OnIcicleFalling;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; //떨어지기 전까지 고정
    }

    public void StartIcicle(float delay)
    {
        OnMoveBlocked?.Invoke();   //생성 즉시 이동 금지
        StartCoroutine(FallRoutine(delay));
    }

    private IEnumerator FallRoutine(float delay)
    {
        Vector3 startPos = transform.position;
        Vector3 warningPos = startPos + Vector3.down * warningMoveDistance;

        float timer = 0f;
        bool moveUnlocked = false;

        while (timer < delay)
        {
            timer += Time.deltaTime;
            float t = timer / delay;

            transform.position = Vector3.Lerp(startPos, warningPos, t);

            //낙하 1초 전 이동 허용
            if (!moveUnlocked && timer >= delay - 1f)
            {
                moveUnlocked = true;
                OnMoveAllowed?.Invoke();
            }

            yield return null;
        }

        sr.sprite = fallingSprite;
        rb.isKinematic = false;
        OnIcicleFalling?.Invoke();
    }

}
