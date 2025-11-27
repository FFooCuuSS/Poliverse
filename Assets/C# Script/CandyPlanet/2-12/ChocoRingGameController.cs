using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChocoRingGameController : MonoBehaviour
{
    public SpawnRing spawner;
    public BowlController bowl;

    public int[] possibleCounts = { 7, 10, 15 };
    int targetCount;

    public Transform bowlShowPos;
    public float bowlMoveSpeed = 3f;

    public GameObject choiceUI;

    private void Start()
    {
        targetCount = possibleCounts[Random.Range(0, possibleCounts.Length)];
        spawner.SpawnRings(targetCount);
        bowl.SetTargetCount(targetCount);
        bowl.OnAllReceived += OnAllChocosCollected;
    }
    void OnAllChocosCollected()
    {
        StartCoroutine(BringBowlUpRoutine());
    }

    IEnumerator BringBowlUpRoutine()
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
        choiceUI.SetActive(true);
    }
    
    public void OnSelectChoice(int choice)
    {
        if(choice == targetCount)
        {
            Debug.Log("정답");
        }
        else
        {
            Debug.Log("오답");
        }
    }
}
