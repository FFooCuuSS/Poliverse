using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl3_8 : MonoBehaviour
{
    [SerializeField] private Minigame3_8remake game; // 인스펙터로 연결

    [Header("스프라이트")]
    public Sprite normalSprite;   // 기본 모습
    public Sprite lookSprite;     // 잠깐 바뀔 모습

    [Header("시간 설정")]
    public float lookDuration = 0.5f; // look 유지 시간

    private SpriteRenderer sr;
    private Coroutine changeCoroutine; // 현재 실행 중인 코루틴 저장

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 시작 시 normal로 맞춤
        if (sr != null && normalSprite != null)
        {
            sr.sprite = normalSprite;
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            game.SubmitInput();

            PlayWatchRoutine();
        }
    }

    // 외부에서 이 함수만 호출하면 됨
    public void PlayWatchRoutine()
    {
        // 이미 실행 중인 코루틴이 있으면 중지
        // -> "가장 최근 bush 생성 기준으로 다시 2초 카운트" 하게 됨
        if (changeCoroutine != null)
        {
            StopCoroutine(changeCoroutine);
        }

        changeCoroutine = StartCoroutine(ChangeRoutine());
    }

    private IEnumerator ChangeRoutine()
    {

        // enemy look으로 변경
        if (sr != null && lookSprite != null)
        {
            sr.sprite = lookSprite;
        }

        // 0.5초 유지
        yield return new WaitForSeconds(lookDuration);

        // 다시 normal로 변경
        if (sr != null && normalSprite != null)
        {
            sr.sprite = normalSprite;
        }

        changeCoroutine = null;
    }
}
