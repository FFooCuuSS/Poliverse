using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager : MonoBehaviour
{
    public GameObject BossStage;

    public GameObject ScoreText;
    public GameObject UpPlatform;
    public GameObject DownPlatform;
    public GameObject Police;
    public GameObject Sinner;
    //public GameObject RandomTextObj;

    public Transform spawnParent; 
    private GameObject currentPerson;

    private Score score;
    private Minigame_1_10 minigame_1_10;
    //public RandomText randomText;

    public bool spawnMan;
    public bool platformIsMoving = false;

    private bool isSinner;
    private GameObject upPlatformInstance;
    private GameObject downPlatformInstance;

    public enum PlatformType { Up, Down }

    private void Start()
    {
        spawnMan = true;

        minigame_1_10 = BossStage.GetComponent<Minigame_1_10>();
        score = ScoreText.GetComponent<Score>();
        //randomText = RandomTextObj.GetComponent<RandomText>();

        SpawnPlatform(PlatformType.Up);
        SpawnPlatform(PlatformType.Down);
    }

    private void Update()
    {
        if (spawnMan)
        {
            SpawnPerson();
            spawnMan = false;
        }
    }

    public void SpawnPlatform(PlatformType type)
    {
        GameObject prefab = type == PlatformType.Up ? UpPlatform : DownPlatform;
        GameObject instance = Instantiate(prefab, prefab.transform.position, Quaternion.identity, spawnParent);
        UpDown upDown = instance.GetComponent<UpDown>();
        upDown.ManagerObj = this.gameObject;

        if (currentPerson != null)
            Destroy(currentPerson);

        spawnMan = true;
    }

    public void SpawnPerson()
    {
        bool spawnSinner = Random.Range(0, 2) == 0;
        GameObject prefab = spawnSinner ? Sinner : Police;
        isSinner = spawnSinner;
        currentPerson = Instantiate(prefab, prefab.transform.position, Quaternion.identity, spawnParent);

        //randomText.ShowLine(!isSinner);
    }

    public void MovePerson(bool goUp)
    {
        if (currentPerson == null) return;

        float targetX = goUp ? 5f : -5f;
        Vector2 targetPos = new Vector2(targetX, currentPerson.transform.position.y);

        currentPerson.transform.DOMove(targetPos, 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                StartCoroutine(MoveUpOrDownAfterDelay(goUp));
            });

        if ((goUp && !isSinner) || (!goUp && isSinner))
        {
            Success();
        }
        else
        {
            Failure();
        }
    }

    private IEnumerator MoveUpOrDownAfterDelay(bool goUp)
    {
        yield return new WaitForSeconds(0.15f);

        Vector2 finalPos = currentPerson.transform.position + Vector3.up * (goUp ? 12f : -12f);

        currentPerson.transform.DOMove(finalPos, 0.3f)
            .SetEase(Ease.OutQuad);
    }

    private void Success()
    {
        score.nScore++;
        spawnMan = false;
        if (score.nScore >= 15)
        {
            minigame_1_10.Succeed();
        }
    }

    private void Failure()
    {
        // ½ÇÆÐ
    }
}
