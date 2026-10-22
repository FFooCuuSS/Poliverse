using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWatch3_8 : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    [Header("효과음")]
    [SerializeField] private AudioClip walkSound;
    [Header("스프라이트")]
    public Sprite normalSprite;   // 기본 모습
    public Sprite lookSprite;     // 잠깐 바뀔 모습

    [Header("시간 설정")]
    public float changeTime = 2.4f;     // bush 생성 후 몇 초 뒤 바뀔지
    public float lookDuration = 0.6f; // look 유지 시간

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
        // 범인이 뒤돌아보기 전까지 대기
        yield return new WaitForSeconds(changeTime);

        if (sr != null && lookSprite != null)
        {
            // 범인이 뒤돌아봄
            sr.sprite = lookSprite;

            if (clickSound != null &&
                GameRoot.Instance != null &&
                GameRoot.Instance.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(clickSound);
            }

            // 실제로 뒤돌아보는 순간 데모 클릭
            Minigame3_8remake game =
                FindObjectOfType<Minigame3_8remake>();

            if (game != null && game.IsDemoMode)
            {
                game.PerformDemoClick();
            }
        }

        // 뒤돌아보는 시간 동안 유지
        yield return new WaitForSeconds(lookDuration);

        if (sr != null && normalSprite != null)
        {
            // 다시 앞을 봄
            sr.sprite = normalSprite;

            if (walkSound != null &&
                GameRoot.Instance != null &&
                GameRoot.Instance.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(walkSound);
            }
        }

        changeCoroutine = null;
    }
}
