using System.Collections;
using UnityEngine;

public class secondGameCommand : MonoBehaviour
{
    private bool isStarted = false;
    private int patternIndex = 0;

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
    private int[][] burstCount = new int[][]
    {
    new int[] { 1, 2, 2 },
    new int[] { 3, 3, 2 },
    new int[] { 6, 1 }
    };
    private float lightRemaining;
    private float wandRemaining;

    public void StartPattern()
    {
        if (isStarted) return;
        isStarted = true;
        StartCoroutine(PatternRoutine());
    }

    private IEnumerator PatternRoutine()
    {
        for (patternIndex = 1; patternIndex <= 3; patternIndex++)
        {
            yield return StartCoroutine(RunPattern(patternIndex));


            if (patternIndex < 3)
                yield return new WaitForSeconds(delayBetweenPatterns);
        }
    }

    private IEnumerator RunPattern(int index)
    {
        switch (index)
        {
            case 1:
                yield return new WaitForSeconds(3f);
                int indexOfPatern = 0;
                lightRemaining = 0.5f;
                wandRemaining = 1.5f;
                for (int i = 0; i < 3; i++)
                {
                    for (int b = 0; b < burstCount[index - 1][i]; b++)
                    {
                        GameObject tempWand = Instantiate(dealingWandPrefab);
                        dealingWand DealingWand = tempWand.GetComponent<dealingWand>();
                        DealingWand.Fire(pattern1[indexOfPatern + b].position, pattern1[indexOfPatern + b].direction, 
                            lightRemaining, wandRemaining/*, new Vector2 (5,0)*/);
                    }
                    indexOfPatern += burstCount[index - 1][i];
                    yield return new WaitForSeconds(3f);
                }
                break;

            case 2:
                indexOfPatern = 0;
                lightRemaining = 2f;
                wandRemaining = 3f;
                for (int i = 0; i < 3; i++)
                {
                    for (int b = 0; b < burstCount[index - 1][i]; b++)
                    {
                        GameObject tempWand = Instantiate(dealingWandPrefab);
                        dealingWand DealingWand = tempWand.GetComponent<dealingWand>();
                        Vector2 dir = pattern2[indexOfPatern + b].direction.normalized;
                        Vector2 opp = new Vector2(-dir.x, dir.y);
                        if (i < 2) DealingWand.Fire(pattern2[indexOfPatern + b].position, pattern2[indexOfPatern + b].direction,
                                   lightRemaining, wandRemaining, opp * 7f);
                        else
                        {
                            lightRemaining = 4f;
                            wandRemaining = 5f;
                            if (b == 0) DealingWand.Fire(pattern2[indexOfPatern + b].position, pattern2[indexOfPatern + b].direction,
                                    lightRemaining, wandRemaining, angleOffsetDeg: -60f);
                            else        DealingWand.Fire(pattern2[indexOfPatern + b].position, pattern2[indexOfPatern + b].direction,
                                    lightRemaining, wandRemaining, angleOffsetDeg: 60f);
                        }
                    }
                    indexOfPatern += burstCount[index - 1][i];
                    yield return new WaitForSeconds(4f);
                }
                yield return new WaitForSeconds(1.5f);
                break;

            case 3:
                indexOfPatern = 0;
                for (int i = 0; i < 2; i++)
                {
                    yield return new WaitForSeconds(5f);
                    for (int b = 0; b < burstCount[index - 1][i]; b++)
                    {
                        lightRemaining = 99f;
                        wandRemaining = 99f;
                        if (i == 0)
                        {
                            GameObject tempWand = Instantiate(dealingWandPrefab);
                            dealingWand DealingWand = tempWand.GetComponent<dealingWand>();
                            DealingWand.Fire(pattern3[indexOfPatern + b].position, pattern3[indexOfPatern + b].direction,
                                lightRemaining, wandRemaining);
                        }
                        else
                        {
                            GameObject tempWand = Instantiate(endingWandPrefab);
                            EndingWand endingWand = tempWand.GetComponentInChildren<EndingWand>(true);
                            endingWand.Fire(pattern3[indexOfPatern + b].position, pattern3[indexOfPatern + b].direction,
                                            lightRemaining, wandRemaining);
                        }
                    }
                    indexOfPatern += burstCount[index - 1][i];
                    yield return new WaitForSeconds(5f);
                }
                break;
        }
    }
}
