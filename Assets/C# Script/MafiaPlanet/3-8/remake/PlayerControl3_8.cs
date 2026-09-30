using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl3_8 : MonoBehaviour
{
    [SerializeField] private Minigame3_8remake game; // 인스펙터로 연결

    [Header("시간 설정")]
    public float lookDuration = 0.6f; // SpriteRenderer가 꺼져있는 시간

    // Player 오브젝트에 붙어있는 SpriteRenderer
    private SpriteRenderer sr;

    // 현재 실행 중인 코루틴
    private Coroutine changeCoroutine;


    void Awake()
    {
        // 현재 Player 오브젝트의 SpriteRenderer 가져오기
        sr = GetComponent<SpriteRenderer>();
    }


    void Start()
    {
        // 시작할 때 Player의 SpriteRenderer 활성화
        // 기존 normalSprite 상태와 동일
        if (sr != null)
        {
            sr.enabled = true;
        }
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse Clicked!");

            PlayWatchRoutine();
        }
    }


    // 클릭했을 때 호출
    public void PlayWatchRoutine()
    {
        // 이미 실행 중인 코루틴이 있다면 중지
        // 다시 클릭하면 0.6초를 처음부터 다시 계산
        if (changeCoroutine != null)
        {
            StopCoroutine(changeCoroutine);
        }

        changeCoroutine = StartCoroutine(ChangeRoutine());
    }


    private IEnumerator ChangeRoutine()
    {
        // 기존 lookSprite 상태
        // Player의 SpriteRenderer를 비활성화
        if (sr != null)
        {
            sr.enabled = false;
        }

        // 0.6초 대기
        yield return new WaitForSeconds(lookDuration);

        // 기존 normalSprite 상태
        // Player의 SpriteRenderer를 다시 활성화
        if (sr != null)
        {
            sr.enabled = true;
        }

        changeCoroutine = null;
    }
}