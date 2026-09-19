using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame2_4 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "같은 색끼리 옮겨담아라!";

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.45f;
    public override float hitWindowOverride => 0.6f;

    private bool ended;
    private readonly Queue<Bottle2_4> pendingBottles = new Queue<Bottle2_4>();
    public bool IsInputOpen => pendingBottles.Count > 0;

    [SerializeField] private BottleSpawner2_4 spawner;
    [SerializeField] private Kettle2_4 kettle;

    [SerializeField] private GameObject liquidPrefab;
    [SerializeField] private Sprite[] fillingSprites;

    // MiniGameBase를 건드리지 않고, 어떤 RhythmManager 구현체든 대응
    private double GetSongTime()
    {
        if (rhythmManager is RhythmManager rm) return rm.SongTime;
        if (rhythmManager is RhythmManagerTest rmt) return rmt.SongTime;
        return -1;
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        pendingBottles.Clear();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        switch (action)
        {
            case "Show":
                var newBottle = spawner.SpawnBottle();
                pendingBottles.Enqueue(newBottle);
                Debug.Log($"[MiniGame2_4] Show @ SongTime {GetSongTime():F3} → 병 생성 (id={newBottle?.GetInstanceID()}), 대기 중: {pendingBottles.Count}개");
                break;
            case "Input":
                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (ended) return;

        Debug.Log($"[MiniGame2_4] 클릭 수신 @ SongTime {GetSongTime():F3}, 대기 중: {pendingBottles.Count}개");

        if (pendingBottles.Count == 0) return;

        kettle.Pour();

        rhythmManager?.ReceivePlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        Debug.Log($"[MiniGame2_4] OnJudgement 호출: {judgement}, 대기 중: {pendingBottles.Count}개");

        // 판정 하나당(클릭이든 타임아웃 자동 Miss든) 큐에서 가장 오래된 병 하나를 처리한다.
        if (pendingBottles.Count == 0)
        {
            Debug.LogWarning("[MiniGame2_4] 판정이 왔는데 대기 중인 병이 없습니다.");
            return;
        }

        Bottle2_4 targetBottle = pendingBottles.Dequeue();

        // Bottle2_4가 컨베이어 끝에서 스스로 Destroy될 수 있으므로,
        // 판정이 오기 전에 이미 파괴된 경우를 방어한다.
        if (targetBottle == null)
        {
            Debug.LogWarning("[MiniGame2_4] 대상 병이 이미 파괴되어 판정을 건너뜁니다.");
            return;
        }

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                CreateRandomFilling(targetBottle);
                break;

            case JudgementResult.Miss:
                // 이 병은 못 채움 (필요하면 여기서 실패 연출 추가)
                break;
        }
    }

    private void CreateRandomFilling(Bottle2_4 targetBottle)
    {
        if (liquidPrefab == null || fillingSprites == null || fillingSprites.Length == 0)
        {
            Debug.LogError("Liquid Prefab 또는 Filling Sprite가 비어있습니다.");
            return;
        }

        int randomIndex = Random.Range(0, fillingSprites.Length);
        Sprite selectedSprite = fillingSprites[randomIndex];

        GameObject liquid = Instantiate(
            liquidPrefab,
            kettle.PourPoint.position,
            Quaternion.identity
        );

        SpriteRenderer liquidRenderer = liquid.GetComponent<SpriteRenderer>();

        if (liquidRenderer != null)
        {
            liquidRenderer.sprite = selectedSprite;
            Debug.Log($"[MiniGame2_4] FillBottle 호출 대상 id={targetBottle.GetInstanceID()}");
            targetBottle.FillBottle(liquid);
        }
        else
        {
            Destroy(liquid);
        }
    }
}