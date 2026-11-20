using System.Collections.Generic;
using UnityEngine;

public class Minigame4_5 : MiniGameBase
{
    [Header("Fish")]
    public Fish4_5 leftFish;
    public Fish4_5 rightFish;

    [Header("Tire")]
    public GameObject tire1Prefab;   // 패턴 A 타이어 (Show1)
    public GameObject tire2Prefab;   // 패턴 B 타이어 (Show2)

    [Header("Hop")]
    [Tooltip("착지 위치 = 복어 위치 + 이 값. 복어와 겹치면 키우고, 떠 보이면 줄이세요 (음수도 가능)")]
    public float landOffsetY = 1f;

    public Transform tireStart;
    public Transform tireExit;          // 성공 시 날아갈 오른쪽 위치
    public float flyDuration = 0.6f;
    public float flyArc = 2f;           // 튀어오르는 높이
    public float fallDistance = 6f;
    public float fallDuration = 0.5f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip sfxFish;
    public AudioClip sfxBounce;

    protected override float TimerDuration => 20f;
    protected override string MinigameTitle => "타이어 복어";
    protected override string MinigameExplain => "타이어가 오면 복어를 눌러 오른쪽으로 보내세요.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "왼쪽 복어, 오른쪽 복어 순서로 정확한 타이밍에 누르면 성공합니다."
    };

    // 판정이 오래 걸리지 않도록 범위를 좁힌다 (Good 범위 0.3초보다 크게)
    public override float hitWindowOverride => 0.4f;

    private readonly Queue<Tire4_5> queue = new Queue<Tire4_5>();
    private int pendingSide = -1;

    public override void StartGame()
    {
        base.StartGame();
        queue.Clear();
        pendingSide = -1;
    }

    public override void OnRhythmEvent(string action)
    {
        switch (action)
        {
            case "Show1":
                SpawnTire(tire1Prefab);
                break;

            case "Show2":
                SpawnTire(tire2Prefab);
                break;

            case "Input":
                HopToRight();
                break;
        }
    }

    private void SpawnTire(GameObject prefab)
    {
        if (prefab == null || tireStart == null || leftFish == null) return;

        var go = Instantiate(prefab, tireStart.position, Quaternion.identity);
        var t = go.GetComponent<Tire4_5>();
        if (t == null) return;

        queue.Enqueue(t);
        t.Hop(LandPoint(leftFish), t.hop1Duration, t.hop1Arc);
    }

    public void OnFishClicked(Fish4_5 fish)
    {
        if (IsInputLocked) return;

        fish.Puff();
        PlaySfx(sfxFish);

        if (queue.Count == 0) return;

        var tire = queue.Peek();
        if (tire == null) return;

        int side = fish.isLeft ? 0 : 1;

        // 이미 판정이 끝난 쪽, 또는 왼쪽이 끝나기 전의 오른쪽 클릭은 판정하지 않는다
        if (tire.IsSideDone(side)) return;
        if (side == 1 && !tire.IsSideDone(0)) return;

        pendingSide = side;
        OnPlayerInput(fish.isLeft ? "Input" : "Tap");
        pendingSide = -1;
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);   // 점수 집계라서 반드시 호출
        if (queue.Count == 0) return;

        var t = queue.Peek();
        if (t == null) { queue.Dequeue(); return; }

        bool done = t.AddJudgement(pendingSide, judgement);

        // 하나라도 Miss면 기다리지 않고 지금 있는 자리에서 바로 떨어진다
        if (t.Failed && !t.Resolved)
        {
            t.Resolved = true;
            t.MoveTo(t.transform.position + Vector3.down * fallDistance, fallDuration, 0f, true, false);
        }

        if (!done) return;

        queue.Dequeue();

        // 이미 떨어진 타이어는 정리만 한다
        if (t.Resolved) { t.Retire(); return; }

        t.Resolved = true;
        PlaySfx(sfxBounce);
        Vector3 exit = tireExit != null ? tireExit.position : t.transform.position + Vector3.right * 10f;
        t.MoveTo(exit, flyDuration, flyArc, false);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private void HopToRight()
    {
        if (rightFish == null) return;

        foreach (var t in queue)
        {
            if (t == null || t.Resolved || t.HopCount != 1) continue;

            t.Hop(LandPoint(rightFish), t.hop2Duration, t.hop2Arc);
            return;
        }
    }

    private Vector3 LandPoint(Fish4_5 fish)
    {
        return fish.transform.position + Vector3.up * landOffsetY;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        foreach (var c in Physics2D.OverlapPointAll(p))
        {
            var fish = c.GetComponentInParent<Fish4_5>();
            if (fish == null) continue;

            OnFishClicked(fish);
            return;
        }
    }
}