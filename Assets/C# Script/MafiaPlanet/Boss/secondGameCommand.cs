using System.Collections;
using UnityEngine;

public class secondGameCommand : MonoBehaviour
{
    private bool isStarted = false;
    private int patternIndex = 0;
    [SerializeField] private Transform wandParent;
    [SerializeField] GameObject mirror;
    [SerializeField] GameObject stage_3_15;
    private Minigame_3_15 minigame_3_15;
    private Mirror_3_15 mirror_3_15;

    [System.Serializable]
    public struct WandPatternData
    {
        public Vector2 position;
        public Vector2 direction;

        public WandPatternData(Vector2 pos, Vector2 dir)
        {
            position = pos;
            direction = dir;
        }
    }

    [Header("Pattern Settings")]
    [SerializeField] private float delayBetweenPatterns = 1f;
    [SerializeField] private GameObject dealingWandPrefab;
    [SerializeField] private GameObject endingWandPrefab;

    [SerializeField] private WandPatternData[] pattern1;
    [SerializeField] private WandPatternData[] pattern2;
    [SerializeField] private WandPatternData[] pattern3;
    private readonly int[][] burstCount = new int[][]
    {
        new int[] { 1, 2, 2 },
        new int[] { 3, 3, 2 },
        new int[] { 6, 1 }
    };
    private float lightRemaining;
    private float wandRemaining;

    [Header("Boss Sprite (레이저 발사 중에만 교체)")]
    [SerializeField] private SpriteRenderer bossRenderer;
    [SerializeField] private Sprite bossLaserSprite;
    [Tooltip("패턴 3(거울)에서 레이저 중일 때 쓸 스프라이트. 비우면 Boss Laser Sprite 사용")]
    [SerializeField] private Sprite bossMirrorSprite;
    [SerializeField] private bool swapOnDealingWand = true;
    [SerializeField] private bool swapOnEndingWand = true;
    private Sprite bossDefaultSprite;
    private int activeLaserCount = 0;

    private void Start()
    {
        minigame_3_15 = stage_3_15.GetComponent<Minigame_3_15>();
        mirror_3_15 = mirror.GetComponent<Mirror_3_15>();

        if (bossRenderer != null)
            bossDefaultSprite = bossRenderer.sprite;
    }

    public void StartPattern()
    {
        if (isStarted) return;
        isStarted = true;
        StartCoroutine(PatternRoutine());
    }

    // ───── 보스 스프라이트 ─────
    private void OnLaserStateChanged(bool on)
    {
        activeLaserCount = Mathf.Max(0, activeLaserCount + (on ? 1 : -1));
        RefreshBossSprite();
    }

    private void RefreshBossSprite()
    {
        if (bossRenderer == null) return;

        Sprite laserSprite = (patternIndex == 3 && bossMirrorSprite != null)
            ? bossMirrorSprite
            : bossLaserSprite;

        if (activeLaserCount > 0 && laserSprite != null)
            bossRenderer.sprite = laserSprite;
        else
            bossRenderer.sprite = bossDefaultSprite;
    }

    // ───── 스폰 헬퍼 ─────
    private dealingWand SpawnDealingWand()
    {
        GameObject go = Instantiate(dealingWandPrefab, wandParent);
        dealingWand w = go.GetComponent<dealingWand>();
        if (swapOnDealingWand) w.OnLaserStateChanged += OnLaserStateChanged;
        return w;
    }

    private EndingWand SpawnEndingWand()
    {
        GameObject go = Instantiate(endingWandPrefab, wandParent);
        EndingWand w = go.GetComponentInChildren<EndingWand>(true);
        if (swapOnEndingWand) w.OnLaserStateChanged += OnLaserStateChanged;
        return w;
    }

    private IEnumerator PatternRoutine()
    {
        for (patternIndex = 1; patternIndex <= 3; patternIndex++)
        {
            RefreshBossSprite(); // 패턴 바뀔 때 스프라이트 종류 갱신
            yield return StartCoroutine(RunPattern(patternIndex));

            if (patternIndex < 3)
                yield return new WaitForSeconds(delayBetweenPatterns);
        }
    }

    private IEnumerator RunPattern(int index)
    {
        int indexOfPatern = 0;

        switch (index)
        {
            case 1:
                yield return new WaitForSeconds(3f);
                lightRemaining = 0.5f;
                wandRemaining = 1.5f;
                for (int i = 0; i < 3; i++)
                {
                    for (int b = 0; b < burstCount[index - 1][i]; b++)
                    {
                        var p = pattern1[indexOfPatern + b];
                        SpawnDealingWand().Fire(p.position, p.direction, lightRemaining, wandRemaining);
                    }
                    indexOfPatern += burstCount[index - 1][i];
                    yield return new WaitForSeconds(3f);
                }
                break;

            case 2:
                lightRemaining = 2f;
                wandRemaining = 3f;
                for (int i = 0; i < 3; i++)
                {
                    for (int b = 0; b < burstCount[index - 1][i]; b++)
                    {
                        var p = pattern2[indexOfPatern + b];
                        dealingWand w = SpawnDealingWand();

                        if (i < 2)
                        {
                            Vector2 dir = p.direction.normalized;
                            Vector2 opp = new Vector2(-dir.x, dir.y);
                            w.Fire(p.position, p.direction, lightRemaining, wandRemaining, opp * 7f);
                        }
                        else
                        {
                            lightRemaining = 4f;
                            wandRemaining = 5f;
                            float angle = (b == 0) ? -60f : 60f;
                            w.Fire(p.position, p.direction, lightRemaining, wandRemaining, angleOffsetDeg: angle);
                        }
                    }
                    indexOfPatern += burstCount[index - 1][i];
                    yield return new WaitForSeconds(4f);
                }
                yield return new WaitForSeconds(1.5f);
                break;

            case 3:
                lightRemaining = 99f;
                wandRemaining = 99f;
                for (int i = 0; i < 2; i++)
                {
                    yield return new WaitForSeconds(3f);
                    for (int b = 0; b < burstCount[index - 1][i]; b++)
                    {
                        var p = pattern3[indexOfPatern + b];

                        if (i == 0)
                        {
                            SpawnDealingWand().Fire(p.position, p.direction, lightRemaining, wandRemaining);
                            mirror_3_15.SummonTo();
                        }
                        else
                        {
                            EndingWand endingWand = SpawnEndingWand();
                            endingWand.Fire(p.position, p.direction, lightRemaining, wandRemaining);

                            float x = mirror.transform.position.x;
                            if (x >= -1f && x <= 1f)
                            {
                                Invoke(nameof(delayedSuccess), 3f);
                                mirror_3_15.SetChildActive();
                                endingWand.EnableNotify(); // 플레이어 피격 OFF
                            }
                            // 거울 실패 시: 레이저에 닿을 때마다 PlayerDrag 쿨다운 간격으로 Miss
                        }
                    }

                    indexOfPatern += burstCount[index - 1][i];
                    yield return new WaitForSeconds(3f);
                }
                break;
        }
    }

    void delayedSuccess()
    {
        minigame_3_15.ReportHit("페이즈2 거울 반사 성공");
    }
}