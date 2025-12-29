using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager : MonoBehaviour
{
    private MiniGameBase minigame;

    public GameObject BossStage;

    public GameObject ScoreText;
    public GameObject UpPlatform;
    public GameObject DownPlatform;
    public GameObject Police;
    public GameObject Sinner;
    //public GameObject RandomTextObj;
    [SerializeField] private Sprite[] sinnerSprites;

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

        minigame = GetComponentInParent<MiniGameBase>();
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

        SpriteRenderer sr = currentPerson.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (isSinner && sinnerSprites.Length > 0)
            {
                int randIndex = Random.Range(0, sinnerSprites.Length);
                sr.sprite = sinnerSprites[randIndex];
            }

            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            StartCoroutine(FadeIn(sr, 0.1f));
        }

        //randomText.ShowLine(!isSinner);
    }

    private IEnumerator FadeIn(SpriteRenderer sr, float duration)
    {
        float timer = 0f;
        Color originalColor = sr.color;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // 보정
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
        if (score.nScore >= 10)
        {
            minigame_1_10.Succeed();
        }

        minigame.OnPlayerInput();
    }

    private void Failure()
    {
        // 실패
    }
}
