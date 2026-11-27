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
    public Transform landPoint1;
    public Transform landPoint2;
    public Transform tireStart;

    [Header("다음 단계에서 쓸 값 (지금은 안 씀)")]
    public Transform tireExit;
    public float flyDuration = 0.6f;
    public float flyArc = 2f;
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

    public override float hitWindowOverride => 0.4f;

    private readonly Queue<Tire4_5> queue = new Queue<Tire4_5>();

    public override void StartGame()
    {
        base.StartGame();
        queue.Clear();
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

            case "Move":
                HopToRight();
                break;
        }
    }

    private void SpawnTire(GameObject prefab)
    {
        if (prefab == null || tireStart == null || landPoint1 == null) return;

        var go = Instantiate(prefab, tireStart.position, Quaternion.identity);
        var t = go.GetComponent<Tire4_5>();
        if (t == null) return;

        queue.Enqueue(t);
        t.NextHop(landPoint1.position);
    }
    private void HopToRight()
    {
        if (landPoint2 == null) return;

        foreach (var t in queue)
        {
            if (t == null || t.HopCount != 1) continue;

            t.NextHop(landPoint2.position);
            return;
        }
    }

    public void OnFishClicked(Fish4_5 fish)
    {
        if (IsInputLocked) return;

        fish.Puff();
        if (audioSource != null && sfxFish != null) audioSource.PlayOneShot(sfxFish);
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