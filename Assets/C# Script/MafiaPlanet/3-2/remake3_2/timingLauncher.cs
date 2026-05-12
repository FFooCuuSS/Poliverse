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

    public GameObject suitcase;
    public GameObject player;

    bool isTestScene = true;
    bool isMovingObject = false;
    private void Start()
    {
        TestScene.SetActive(true);
        PlayScene.SetActive(false);
    }
    
    // Update is called once per frame
    void Update()
    {
        

        if(isTestScene)
        {
            timer += Time.deltaTime;
            if (!isMovingObject && timer>=1.5f)
            {
                ChangeSprite();
                MoveObject();
                timer = 0f;
                isMovingObject = true;
            }
            if(timer>=2f)
            {
                isTestScene = false;
            }
            
        }
    }
    void Clear()
    {
        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = originSprite;
        suitcase.transform.position = enemyObject.transform.position;
    }
    void MoveObject()
    {
        StartCoroutine(MoveRoutine());
    }
    IEnumerator MoveRoutine()
    {
        Vector3 startPosition = suitcase.transform.position;
        Vector3 endPosition = player.transform.position;
        float duration = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            suitcase.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        suitcase.transform.position = endPosition;
    }
    void ChangeSprite()
    {
        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = changeSprite;
    }
}
