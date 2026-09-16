using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timingLauncher : MonoBehaviour
{
    public GameObject TestScene;
    public GameObject PlayScene;

    public GameObject enemyObject;
    public Sprite changeSprite;
    public Sprite originSprite;

    private float timer = 0f;
    public int counter = 0;

    // suitcase는 씬에 있는 오브젝트가 아니라 프리팹으로 넣는 용도
    public GameObject suitcase;

    public GameObject player;

    bool isTestScene = true;
    bool isMovingObject = false;

    // 현재 생성된 suitcase 저장
    GameObject currentSuitcase;

    void Start()
    {
        counter=0;
        TestScene.SetActive(true);
        PlayScene.SetActive(false);
    }

    void Update()
    {
        if(counter>=5)
        {
            return;
        }
        if (isTestScene)
        {
            timer += Time.deltaTime;

            if (!isMovingObject && timer >= 1.5f)
            {
                ChangeSprite();

                // enemyObject 위치에 suitcase 생성 후 이동
                SpawnAndMoveSuitcase();

                timer = 0f;
                isMovingObject = true;
            }

            if (timer >= 2f)
            {
                isTestScene = false;
                Clear();
                timer = 0f;
                isMovingObject = false;
            }
        }
        if (!isTestScene)
        {
            timer += Time.deltaTime;
            if (timer >= 1.5f)
            {
                ChangeSprite();
                SpawnAndMoveSuitcase();
                timer = 0f;
                isMovingObject = true;
            }
            if (timer >= 2f)
            {
                Clear();
                timer = 0f;
                isMovingObject = false;
            }
        }


        // 좌클릭하면 현재 suitcase 파괴
        if (Input.GetMouseButtonDown(0))
        {
            DestroySuitcase();
        }

        if (!isTestScene)
        {
            TestScene.SetActive(false);
            PlayScene.SetActive(true);
        }
    }

    void Clear()
    {
        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = originSprite;

        // 현재 생성된 suitcase가 있으면 enemy 위치로 이동
        if (currentSuitcase != null)
        {
            currentSuitcase.transform.position = enemyObject.transform.position;
        }
    }

    void SpawnAndMoveSuitcase()
    {
        // enemyObject 위치에서 suitcase 생성
        currentSuitcase = Instantiate(
            suitcase,
            enemyObject.transform.position,
            Quaternion.identity,
            transform
        );

        StartCoroutine(MoveRoutine(currentSuitcase));

        // 생성 후 1초 뒤 파괴
        StartCoroutine(DestroyAfterOneSecond(currentSuitcase));
        counter++;
    }

    IEnumerator MoveRoutine(GameObject targetSuitcase)
    {
        Vector3 startPosition = targetSuitcase.transform.position;
        Vector3 endPosition = player.transform.position;

        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 중간에 파괴되면 코루틴 종료
            if (targetSuitcase == null)
            {
                yield break;
            }

            targetSuitcase.transform.position =
                Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (targetSuitcase != null)
        {
            targetSuitcase.transform.position = endPosition;
        }
    }

    IEnumerator DestroyAfterOneSecond(GameObject targetSuitcase)
    {
        yield return new WaitForSeconds(1f);

        if (targetSuitcase != null)
        {
            Destroy(targetSuitcase);
        }
    }

    void DestroySuitcase()
    {
        if (currentSuitcase != null)
        {
            Destroy(currentSuitcase);
            currentSuitcase = null;
        }
    }

    void ChangeSprite()
    {
        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = changeSprite;
    }
}