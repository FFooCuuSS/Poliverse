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
    [SerializeField] private WandPatternData[] pattern1;

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
                for (int i = 0 ; i < pattern1.Length; i++)
                {
                    GameObject tempWand = Instantiate(dealingWandPrefab);
                    dealingWand DealingWand = tempWand.GetComponent<dealingWand>();
                    DealingWand.Fire(pattern1[i].position, pattern1[i].direction);
                    Debug.Log("실행");
                    yield return new WaitForSeconds(1f);
                }
                break;

            case 2:
                // 패턴 2 실행 코드
                yield return new WaitForSeconds(1.5f);
                break;

            case 3:
                // 패턴 3 실행 코드
                yield return new WaitForSeconds(2f);
                break;
        }
    }
}
