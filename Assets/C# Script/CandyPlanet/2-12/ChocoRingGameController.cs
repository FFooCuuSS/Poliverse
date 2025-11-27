using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChocoRingGameController : MonoBehaviour
{
    private Minigame_2_12 minigame_2_12;
    public GameObject stage_2_12;

    public SpawnRing spawner;
    public BowlController bowl;

    public int[] possibleCounts = { 7, 10, 15 };
    int targetCount;

    public Transform bowlShowPos;
    public float bowlMoveSpeed = 3f;

    public GameObject choiceUI;

    private void Start()
    {
        minigame_2_12 = stage_2_12.GetComponent<Minigame_2_12>();
        targetCount = possibleCounts[Random.Range(0, possibleCounts.Length)];
        Debug.Log(targetCount);
        spawner.SpawnRings(targetCount);//
        bowl.SetTargetCount(targetCount);//
        bowl.OnAllReceived += OnAllChocosCollected;
    }
    void OnAllChocosCollected()
    {
        StartCoroutine(BringBowlUpRoutine(() => { }));
    }

    IEnumerator BringBowlUpRoutine(System.Action onComplete)
    {
        yield return new WaitForSeconds(1.0f);

        while (Vector3.Distance(bowl.transform.position, bowlShowPos.position) > 1.0f)
        {
            bowl.transform.position = Vector3.MoveTowards(
                bowl.transform.position,
                bowlShowPos.position,
                bowlMoveSpeed * Time.deltaTime );
            yield return null;
        }
        onComplete?.Invoke();
        
    }
    
    public void OnSelectChoice(int choice)
    {
        StartCoroutine(BringBowlUpRoutine(()=> {
            if (choice == targetCount)
            {
                Debug.Log("정답");
                minigame_2_12.Succeed();
                return;
            }
            else
            {
                Debug.Log("오답");
                minigame_2_12.Fail();
                return;
            }
        }));
        
    }
}
