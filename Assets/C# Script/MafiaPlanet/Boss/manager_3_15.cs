using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class manager_3_15 : MonoBehaviour
{
    [SerializeField] private GameObject[] gamePrefabs;
    [SerializeField] private GameObject DarkPanel;
    [SerializeField] private Minigame_3_15 minigame;

    [Header("BossSFX")]
    public AudioSource audioSource;
    public AudioClip ohYesClip;

    [Header("Transition Tunables")]
    [SerializeField] private float fadeInSpeed = 0.8f;
    [SerializeField] private float holdTime = 6.5f;
    [SerializeField] private float fadeOutDur = 0.35f;

    [Header("Phase 1")]
    [SerializeField] private int wandGoal = 10;
    public int wandCount = 0;

    private enum State { Idle, MG1, Transition, MG2 }
    private State state = State.Idle;

    private Image darkImg;
    private Coroutine transCo;
    private secondGameCommand sGC;
    private weaponSpawner_3_15[] spawners = new weaponSpawner_3_15[0];

    private void Awake()
    {
        darkImg = DarkPanel != null ? DarkPanel.GetComponent<Image>() : null;
        sGC = GetComponent<secondGameCommand>();
        if (minigame == null) minigame = GetComponentInParent<Minigame_3_15>();

        SetPanelAlpha(0f);

        if (gamePrefabs != null && gamePrefabs.Length >= 2)
        {
            gamePrefabs[0]?.SetActive(true);
            gamePrefabs[1]?.SetActive(false);
        }

        if (gamePrefabs != null && gamePrefabs.Length >= 1 && gamePrefabs[0] != null)
            spawners = gamePrefabs[0].GetComponentsInChildren<weaponSpawner_3_15>(true);

        // StartGame 전에는 스폰 금지
        SetSpawnersBanned(true);
    }

    // Minigame_3_15.StartGame()에서만 호출
    public void BeginGame()
    {
        if (state != State.Idle) return;

        wandCount = 0;
        state = State.MG1;
        SetSpawnersBanned(false);
    }

    // weapon_3_15 클릭 시 호출
    public void OnWeaponClicked(bool isMagicWand)
    {
        if (state != State.MG1) return;

        if (isMagicWand)
        {
            wandCount++;
            minigame?.ReportHit($"페이즈1 마법봉 클릭 ({wandCount}/{wandGoal})");

            if (wandCount >= wandGoal)
                EnterTransition();
        }
        else
        {
            minigame?.ReportMiss("페이즈1 일반 무기 클릭");
        }
    }

    private void SetSpawnersBanned(bool ban)
    {
        foreach (var s in spawners)
            if (s != null) s.banMoving = ban;
    }

    private void EnterTransition()
    {
        state = State.Transition;
        SetSpawnersBanned(true);

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

        // 2) 대기 + SFX
        yield return new WaitForSeconds(holdTime);
        BossSFX();

        // 3) 페이드 아웃
        float startA = darkImg != null ? darkImg.color.a : 1f;
        float t = 0f;
        while (t < fadeOutDur)
        {
            t += Time.deltaTime;
            SetPanelAlpha(Mathf.Lerp(startA, 0f, t / fadeOutDur));
            yield return null;
        }
        SetPanelAlpha(0f);

        // 4) 2페이즈 시작 (한 번만)
        state = State.MG2;
        transCo = null;
        if (sGC != null) sGC.StartPattern();
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
}