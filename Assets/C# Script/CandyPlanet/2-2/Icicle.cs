using System;
using System.Collections;
using UnityEngine;

public class Icicle : MonoBehaviour
{
    public static event Action OnMoveAllowed;
    public static event Action OnMoveBlocked;
    public static event Action<Icicle> OnIcicleDestroyed;

    private int beatCount = 0; // 추가: 박자 카운트
    private bool isFalling = false;

    private float beatTimer = 0f;
    [SerializeField] private float roundTripTime = 0.5f;

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

    // 외부에서 박자(beat) 정보를 받아 낙하 시점 결정
    public void StartIcicle(float delay)
    {
        OnMoveBlocked?.Invoke();
        beatCount = 0;
        beatTimer = 0f;
        isFalling = false;
    }
    void Update()
    {
        if (isFalling) return;

        beatTimer += Time.deltaTime;
        if (beatTimer >= roundTripTime)
        {
            beatTimer = 0f;
            beatCount++;
            Debug.Log($"고드름 박자: {beatCount}");

            // 1박자: 생성됨(이미 됨), 2박자: 낙하 시작
            if (beatCount >= 2)
            {
                StartCoroutine(DropRoutine());
            }
        }
    }

    private IEnumerator DropRoutine()
    {
        isFalling = true;
        sr.sprite = fallingSprite;
        rb.isKinematic = false;
        OnIcicleFalling?.Invoke(); // SpawnIcicle에게 다음 고드름 생성 신호 전달
        yield break;
    }
    
}
