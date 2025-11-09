using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class manager_3_15 : MonoBehaviour
{
    [SerializeField] private GameObject[] gamePrefabs;
    [SerializeField] private GameObject DarkPanel;

    [Header("BossSFX")]
    public AudioSource audioSource;
    public AudioClip ohYesClip;

    [Header("Transition Tunables")]
    [SerializeField] private float fadeInSpeed = 0.8f;
    [SerializeField] private float holdTime = 6.5f;
    [SerializeField] private float fadeOutDur = 0.35f;

    // 1번 미니게임
    public int wandCount = 0;

    // 내부 상태
    private enum State { MG1, Transition, MG2 }
    private State state = State.MG1;

    private Image darkImg;
    private Coroutine transCo;
    private secondGameCommand sGC;

    private void Awake()
    {
        darkImg = DarkPanel.GetComponent<Image>();
        sGC = GetComponent<secondGameCommand>();

        // 시작 시 패널 투명화
        if (darkImg != null)
        {
            var c = darkImg.color;
            c.a = 0f;
            darkImg.color = c;
        }

        if (gamePrefabs != null && gamePrefabs.Length >= 2)
        {
            gamePrefabs[0]?.SetActive(true);
            gamePrefabs[1]?.SetActive(false);
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.MG1:
                if (wandCount >= 10)
                {
                    EnterTransition();
                }
                break;

            case State.Transition:
                break;

            case State.MG2:
                sGC.StartPattern();
                break;
        }
    }

    private void EnterTransition()
    {
        state = State.Transition;

        for (int i = 0; i < 3; i++)
        {
            var tempForBan = gamePrefabs[0].transform.GetChild(i);
            weaponSpawner_3_15 wS = tempForBan.GetComponent<weaponSpawner_3_15>();
            wS.banMoving = true;
        }

        if (transCo != null) StopCoroutine(transCo);
        transCo = StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1) 페이드 인
        float a = darkImg != null ? darkImg.color.a : 0f;
        while (a < 1f)
        {
            a += fadeInSpeed * Time.deltaTime;
            SetPanelAlpha(Mathf.Clamp01(a));
            yield return null;
        }

        if (gamePrefabs != null && gamePrefabs.Length >= 1)
            gamePrefabs[0]?.SetActive(false);

        // 2) 대기 + SFX 실행
        yield return new WaitForSeconds(holdTime);
        BossSFX();

        // 3) 빠른 페이드 아웃
        float startA = darkImg != null ? darkImg.color.a : 1f;
        float t = 0f;
        while (t < fadeOutDur)
        {
            t += Time.deltaTime;
            float k = t / fadeOutDur;
            SetPanelAlpha(Mathf.Lerp(startA, 0f, k));
            yield return null;
        }
        SetPanelAlpha(0f);

        // 4) 상태 전환
        state = State.MG2;
        transCo = null;
    }

    private void SetPanelAlpha(float alpha)
    {
        if (darkImg == null) return;
        var c = darkImg.color;
        c.a = alpha;
        darkImg.color = c;
    }

    private void BossSFX()
    {
        if (audioSource != null && ohYesClip != null)
        {
            audioSource.clip = ohYesClip;
            audioSource.Play();
        }
        
        if (gamePrefabs != null && gamePrefabs.Length >= 2)
            gamePrefabs[1]?.SetActive(true);
    }

    public void OnMagicWandCollected()
    {
        Debug.Log(++wandCount);
    }
}
