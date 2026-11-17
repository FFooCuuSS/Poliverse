using System.Collections.Generic;
using UnityEngine;

public class Waste4_6 : MonoBehaviour
{
    [Tooltip("부수는 데 필요한 프레스 횟수 (1번용 프리팹은 1, 2번용은 2)")]
    public int requiredPresses = 1;

    [Header("Sprites")]
    public SpriteRenderer sr;
    public Sprite normalSprite;    // 멀쩡한 상태
    public Sprite damagedSprite;   // 약간 부서진 상태
    public Sprite brokenSprite;    // 완전히 부서진 상태

    private enum Phase { Entering, Waiting, Exiting }
    private Phase phase = Phase.Entering;

    private Vector3 startPos;
    private Vector3 endPos;
    private double phaseStartDsp;
    private float duration = 1f;
    private readonly List<MiniGameBase.JudgementResult> results = new List<MiniGameBase.JudgementResult>();

    public bool Arrived => phase != Phase.Entering;
    public double ArrivedDsp { get; private set; }

    private void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(Vector3 center, float arriveDuration)
    {
        startPos = transform.position;
        endPos = center;
        duration = Mathf.Max(0.01f, arriveDuration);
        phaseStartDsp = AudioSettings.dspTime;
        phase = Phase.Entering;
        ApplySprite(normalSprite);
    }

    private void Update()
    {
        if (phase == Phase.Waiting) return;

        float t = (float)((AudioSettings.dspTime - phaseStartDsp) / duration);

        if (phase == Phase.Entering)
        {
            if (t >= 1f) { transform.position = endPos; ArrivedDsp = phaseStartDsp + duration; phase = Phase.Waiting; return; }
            transform.position = Vector3.Lerp(startPos, endPos, t);
            return;
        }

        // Exiting
        if (t >= 1f) { Destroy(gameObject); return; }
        transform.position = Vector3.Lerp(startPos, endPos, t);
    }

    // 판정 하나를 받는다. 필요한 횟수가 다 모이면 true
    public bool AddJudgement(MiniGameBase.JudgementResult result)
    {
        results.Add(result);
        return results.Count >= requiredPresses;
    }

    // 최종 상태 스프라이트를 정하고 왼쪽으로 내보낸다
    public void Resolve(Vector3 exitPos, float exitDuration)
    {
        ApplySprite(GetFinalSprite());

        startPos = transform.position;
        endPos = exitPos;
        duration = Mathf.Max(0.01f, exitDuration);
        phaseStartDsp = AudioSettings.dspTime;
        phase = Phase.Exiting;
    }

    private Sprite GetFinalSprite()
    {
        int perfect = 0;
        int miss = 0;

        foreach (var r in results)
        {
            if (r == MiniGameBase.JudgementResult.Perfect) perfect++;
            else if (r == MiniGameBase.JudgementResult.Miss) miss++;
        }

        if (perfect == results.Count) return brokenSprite;    // 전부 Perfect
        if (miss == results.Count) return normalSprite;       // 전부 Miss
        return damagedSprite;                                  // 나머지 조합
    }

    private void ApplySprite(Sprite s)
    {
        if (sr != null && s != null) sr.sprite = s;
    }
}