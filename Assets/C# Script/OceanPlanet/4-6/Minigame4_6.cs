using System.Collections.Generic;
using UnityEngine;

public class Minigame4_6 : MiniGameBase
{
    [Header("Waste")]
    public GameObject waste1Prefab;   // 1번 눌러야 부서지는 폐기물
    public GameObject waste2Prefab;   // 2번 눌러야 부서지는 폐기물
    public Transform spawnPoint;      // 오른쪽 화면 밖
    public Transform centerPoint;     // 프레스 아래

    public float arriveDuration = 1f;

    [Header("Press")]
    public SpriteRenderer pressRenderer;
    public Sprite pressUpSprite;     // 누르기 전
    public Sprite pressDownSprite;   // 누른 후
    [Tooltip("내려찍은 뒤 다음 클릭까지 막는 시간 (연타 방지)")]
    public float pressDelay = 0.3f;

    [Header("Exit")]
    public Transform exitPoint;      // 왼쪽 화면 밖 (CenterPoint와 같은 Y)
    public float exitDuration = 0.5f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip sfxPress;

    [Header("Debug")]
    [SerializeField] private bool timingLog = true;

    // 판정이 안 눌린 채 오래 걸리지 않도록 판정 범위를 좁힌다 (Good 범위 0.3초보다 크게)
    public override float hitWindowOverride => 0.4f;

    private bool pressReady = true;
    private Waste4_6 lastSpawned;

    protected override float TimerDuration => 20f;
    protected override string MinigameTitle => "폐기물 처리";
    protected override string MinigameExplain => "폐기물이 가운데로 옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "폐기물에 맞는 횟수만큼 타이밍에 맞춰 프레스를 누르세요."
    };

    // 판정 단계에서 앞쪽 폐기물부터 차례로 판정을 받는다
    private readonly Queue<Waste4_6> queue = new Queue<Waste4_6>();

    public override void StartGame()
    {
        base.StartGame();
        queue.Clear();
        pressReady = true;
        lastSpawned = null;
        SetPressSprite(pressUpSprite);
    }

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
            case "Input":
                LogGap();
                break;
        }
    }

    private void Spawn(GameObject prefab)
    {
        if (prefab == null || spawnPoint == null || centerPoint == null) return;

        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        var w = go.GetComponent<Waste4_6>();
        if (w == null) return;

        w.Init(centerPoint.position, arriveDuration);
        queue.Enqueue(w);
        lastSpawned = w;
    }

    private void Update()
    {
        if (IsInputLocked || !pressReady) return;
        if (!Input.GetMouseButtonDown(0)) return;

        pressReady = false;
        SetPressSprite(pressDownSprite);
        PlaySfx(sfxPress);
        OnPlayerInput();

        CancelInvoke(nameof(ReleasePress));
        Invoke(nameof(ReleasePress), pressDelay);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);   // 점수 집계라서 반드시 호출
        if (queue.Count == 0) return;

        var w = queue.Peek();
        if (w == null) { queue.Dequeue(); return; }
        if (!w.AddJudgement(judgement)) return;

        queue.Dequeue();
        Vector3 exitPos = exitPoint != null ? exitPoint.position : w.transform.position + Vector3.left * 10f;
        w.Resolve(exitPos, exitDuration);
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

    // 도착~Input 간격이 기획서대로인지 확인하는 로그 (A는 1.00, B는 1.00과 1.50)
    private void LogGap()
    {
        if (!timingLog || lastSpawned == null || !lastSpawned.Arrived) return;
        Debug.Log($"[4-6] Input: 도착 후 {AudioSettings.dspTime - lastSpawned.ArrivedDsp:F2}초");
    }
}