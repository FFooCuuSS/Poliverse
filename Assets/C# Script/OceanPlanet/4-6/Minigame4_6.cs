using UnityEngine;
using System.Collections.Generic;

public class Minigame4_6 : MiniGameBase
{
    [Header("Waste")]
    public GameObject waste1Prefab;   // 1번 눌러야 부서지는 폐기물 (Show1)
    public GameObject waste2Prefab;   // 2번 눌러야 부서지는 폐기물 (Show2)
    public Transform spawnPoint;      // 오른쪽 화면 밖
    public Transform leftEndPoint;    // 왼쪽 끝 (여기에 닿으면 사라짐)
    [Tooltip("Show 후 프레스 가운데에 도착하기까지 걸리는 시간(초)")]
    public float travelTime = 2f;

    [Header("Press")]
    public SpriteRenderer pressRenderer;
    public Sprite pressUpSprite;     // 누르기 전
    public Sprite pressDownSprite;   // 누른 후
    [Tooltip("내려찍은 뒤 다음 클릭까지 막는 시간 (연타 방지)")]
    public float pressDelay = 0.3f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip sfxPress;

    private bool pressReady = true;

    [Header("Press Zone")]
    [Tooltip("프레스 안쪽을 나타내는 Collider2D (Is Trigger 체크). 프레스의 자식에 두면 위치를 옮겨도 따라가요")]
    public Collider2D pressZone;

    private readonly List<Collider2D> hits = new List<Collider2D>();
    private readonly HashSet<Waste4_6> hitWastes = new HashSet<Waste4_6>();


    private readonly Queue<Waste4_6> waiting = new Queue<Waste4_6>();

    private readonly Queue<Waste4_6> judging = new Queue<Waste4_6>();

    protected override float TimerDuration => 20f;
    protected override string MinigameTitle => "폐기물 처리";
    protected override string MinigameExplain => "폐기물이 가운데로 옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "폐기물에 맞는 횟수만큼 타이밍에 맞춰 프레스를 누르세요."
    };

    public override float hitWindowOverride => 0.4f;

    public override void OnRhythmEvent(string action)
    {
        switch (action)
        {
            case "Show1":
                Spawn(waste1Prefab);
                break;

            case "Show2":
                Spawn(waste2Prefab);
                break;

            case "Move":
                ReleaseFront();
                break;
        }
    }

    private void Spawn(GameObject prefab)
    {
        if (prefab == null || spawnPoint == null || leftEndPoint == null) return;

        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        var w = go.GetComponent<Waste4_6>();
        float stopX = pressZone.bounds.center.x;
        float speed = (spawnPoint.position.x - stopX) / travelTime;
        if (w != null) w.Init(speed, leftEndPoint.position.x, stopX);
        if (w != null) waiting.Enqueue(w);
        if (w != null) judging.Enqueue(w);
    }

    public override void StartGame()
    {
        base.StartGame();
        pressReady = true;
        SetPressSprite(pressUpSprite);
        judging.Clear();
    }

    [Header("Debug")]
    public bool autoPlay;
    public float autoOffset;
    private Waste4_6 autoWaste;
    private int autoIndex = -1;

    private void Update()
    {
        if (IsInputLocked || !pressReady) return;
        if (!Input.GetMouseButtonDown(0) && !AutoShouldPress()) return;
        DoPress();
    }

    private bool AutoShouldPress()
    {
        if (!autoPlay || judging.Count == 0) return false;
        var w = judging.Peek();
        if (w == autoWaste && w.PressCount == autoIndex) return false;
        float target = w.PressCount == 0 ? 1f : 1.5f;
        if (AudioSettings.dspTime < w.ArriveDsp + target + autoOffset) return false;
        autoWaste = w;
        autoIndex = w.PressCount;
        return true;
    }

    private void DoPress()
    {
        pressReady = false;
        SetPressSprite(pressDownSprite);
        PlaySfx(sfxPress);
        if (judging.Count > 0) OnPlayerInput();

        CancelInvoke(nameof(ReleasePress));
        Invoke(nameof(ReleasePress), pressDelay);
    }

    private void ReleasePress()
    {
        pressReady = true;
        SetPressSprite(pressUpSprite);
    }

    private void SetPressSprite(Sprite s)
    {
        if (pressRenderer != null && s != null) pressRenderer.sprite = s;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);
        if (judging.Count == 0) return;

        int points = judgement == JudgementResult.Perfect ? 2 : judgement == JudgementResult.Good ? 1 : 0;
        if (judging.Peek().AddJudgement(points)) judging.Dequeue();
    }

    private void ReleaseFront()
    {
        Debug.Log($"[4-6] Move 신호, 대기 중 {waiting.Count}개");
        if (waiting.Count > 0) waiting.Dequeue().Release();
    }
}