using UnityEngine;

public class Minigame4_4 : MiniGameBase
{
    [Header("Screens")]
    public GameObject screen1Root;
    public GameObject screen2Root;

    [Header("Screen 1 - Jellyfish")]
    [Tooltip("서로 다른 y에 미리 배치해 둔 해파리 (최대 5개)")]
    public Jellyfish4_4[] jellyfishSlots;
    public Transform boltParent;

    public enum InputMode { Toggle, ScreenHalf }

    [Header("Screen 2 - Player")]
    public Transform player;
    public Transform upPoint;
    public Transform downPoint;
    [Tooltip("Toggle: 클릭할 때마다 위/아래 교대, ScreenHalf: 누른 곳이 위쪽이면 위, 아래쪽이면 아래")]
    public InputMode inputMode = InputMode.ScreenHalf;

    [Header("Screen 2 - Electric")]
    [Tooltip("플레이어 위쪽 자리로 가는 즉발 전기")]
    public GameObject boltToUp;
    [Tooltip("플레이어 아래쪽 자리로 가는 즉발 전기")]
    public GameObject boltToDown;
    public GameObject hitEffect;
    public float boltShowTime = 0.3f;
    public float hitLockTime = 0.6f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip sfxJellyFire;   // 첫 화면 전기 발사
    public AudioClip sfxDodge;       // 회피 성공 시 둘째 화면 전기
    public AudioClip sfxShock;       // 감전(실패)
    public AudioClip sfxMove;        // 플레이어 이동

    [Header("Player Sprite")]
    public SpriteRenderer playerRenderer;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite upHitSprite;
    public Sprite downHitSprite;
    [Tooltip("끄면 플레이어 오브젝트는 안 움직이고 이미지만 바뀜 (이미지 안에 위/아래 위치가 이미 그려져 있을 때)")]
    public bool moveByTransform = true;

    private bool hitLocked;

    private bool screen2Active;
    private bool playerUp;



    protected override float TimerDuration => 20f;
    protected override string MinigameTitle => "해파리 전기";
    protected override string MinigameExplain => "해파리가 전기를 쏘는 타이밍을 기억하세요.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "다음 화면에서 같은 타이밍에 클릭해서 전기를 피하세요."
    };

    private int nextJellyfish;

    public override void StartGame()
    {
        base.StartGame();

        nextJellyfish = 0;
        screen2Active = false;

        hitLocked = false;
        HideBolts();
        if (hitEffect != null) hitEffect.SetActive(false);

        foreach (var j in jellyfishSlots)
            if (j != null) j.gameObject.SetActive(false);

        if (screen1Root != null) screen1Root.SetActive(true);
        if (screen2Root != null) screen2Root.SetActive(false);
    }

    public override void OnRhythmEvent(string action)
    {
        switch (action)
        {
            case "Show":
                SpawnAndFire();
                break;
            case "Move":
                EnterScreen2();
                break;
        }
    }

    private void SpawnAndFire()
    {
        if (nextJellyfish >= jellyfishSlots.Length)
        {
            Debug.LogWarning("[4-4] Show가 해파리 슬롯 수보다 많습니다. CSV를 확인하세요.");
            return;
        }

        var j = jellyfishSlots[nextJellyfish++];
        if (j == null) return;

        j.gameObject.SetActive(true);
        j.Fire(boltParent);
        PlaySfx(sfxJellyFire);
    }

    private void EnterScreen2()
    {
        if (screen1Root != null) screen1Root.SetActive(false);
        if (screen2Root != null) screen2Root.SetActive(true);

        screen2Active = true;
        SetPlayerUp(false);
    }

    private void SetPlayerUp(bool up)
    {
        playerUp = up;
        RefreshPlayerSprite();

        if (!moveByTransform) return;
        if (player == null || upPoint == null || downPoint == null) return;
        player.position = up ? upPoint.position : downPoint.position;
    }

    private void Update()
    {
        if (!screen2Active || IsInputLocked || hitLocked) return;
        if (!Input.GetMouseButtonDown(0)) return;

        if (inputMode == InputMode.Toggle) SetPlayerUp(!playerUp);
        else SetPlayerUp(ClickIsUpper());

        OnPlayerInput();
        PlaySfx(sfxMove);
    }

    private bool ClickIsUpper()
    {
        float clickY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        float midY = (upPoint.position.y + downPoint.position.y) * 0.5f;
        return clickY > midY;
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);   // 점수 집계라서 반드시 호출
        if (!screen2Active) return;

        bool miss = judgement == JudgementResult.Miss;

        // 성공이면 플레이어가 없는 자리, 실패면 플레이어가 있는 자리로 발사
        bool targetUp = miss ? playerUp : !playerUp;
        ShowBolt(targetUp);
        PlaySfx(miss ? sfxShock : sfxDodge);

        if (!miss) return;

        hitLocked = true;
        RefreshPlayerSprite();
        if (hitEffect != null) hitEffect.SetActive(true);
        CancelInvoke(nameof(EndHit));
        Invoke(nameof(EndHit), hitLockTime);
    }

    private void ShowBolt(bool up)
    {
        HideBolts();
        GameObject b = up ? boltToUp : boltToDown;
        if (b != null) b.SetActive(true);

        CancelInvoke(nameof(HideBolts));
        Invoke(nameof(HideBolts), boltShowTime);
    }

    private void HideBolts()
    {
        if (boltToUp != null) boltToUp.SetActive(false);
        if (boltToDown != null) boltToDown.SetActive(false);
    }

    private void EndHit()
    {
        hitLocked = false;
        RefreshPlayerSprite();
        if (hitEffect != null) hitEffect.SetActive(false);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private void RefreshPlayerSprite()
    {
        if (playerRenderer == null) return;

        Sprite s;
        if (playerUp) s = hitLocked ? upHitSprite : upSprite;
        else s = hitLocked ? downHitSprite : downSprite;

        if (s != null) playerRenderer.sprite = s;
    }
}