using System;
using System.Collections;
using UnityEngine;

public enum IcicleState
{
    Forming,    // 1단계: 맺힘
    Growing,    // 2단계: 커짐
    Falling     // 3단계: 떨어짐
}

public class Icicle : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite formingSprite;
    [SerializeField] private Sprite growingSprite;
    [SerializeField] private Sprite fallingSprite;

    [Header("Timing")]
    [SerializeField] private float formingTime = 0.5f;
    [SerializeField] private float growingTime = 0.7f;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private IcicleState state;

    public static event Action OnIcicleFalling;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // 떨어지기 전까지 고정
    }

    void OnEnable()
    {
        StartCoroutine(StateRoutine());
    }

    private IEnumerator StateRoutine()
    {
        // 1단계
        state = IcicleState.Forming;
        sr.sprite = formingSprite;
        yield return new WaitForSeconds(formingTime);

        // 2단계
        state = IcicleState.Growing;
        sr.sprite = growingSprite;
        yield return new WaitForSeconds(growingTime);

        // 3단계
        state = IcicleState.Falling;
        sr.sprite = fallingSprite;
        rb.isKinematic = false; 
        OnIcicleFalling?.Invoke();

    }
}
