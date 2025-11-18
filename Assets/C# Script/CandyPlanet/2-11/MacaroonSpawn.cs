using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacaroonSpawn : MonoBehaviour
{
    [Header("Prefab & Sprites")]
    public GameObject macaronPrefab;
    public Sprite[] macaronSprites;

    [Header("Spawn Settings")]
    public Vector2 spawnAreaMin = new Vector2(-3, 1);
    public Vector2 spawnAreaMax = new Vector2(3, 3);
    public float minDistance = 1.0f;

    [Header("Options")]
    [SerializeField] private bool useRandomSprites = false; //리소스 받으면 true변경

    void Start()
    {
        if (macaronSprites.Length == 0)
            return;

        int macaronCount = macaronSprites.Length;

        List<Sprite> shuffledSprites = new List<Sprite>(macaronSprites);
        for (int i = 0; i < shuffledSprites.Count; i++)
        {
            int randIndex = Random.Range(i, shuffledSprites.Count);
            (shuffledSprites[i], shuffledSprites[randIndex]) = (shuffledSprites[randIndex], shuffledSprites[i]);
        }

        List<Vector2> usedPositions = new List<Vector2>();

        for (int i = 0; i < macaronCount; i++)
        {
            Vector2 pos;
            int tries = 0;
            const int maxTries = 100;

            do
            {
                pos = new Vector2(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y)
                );
                tries++;
                if (tries > maxTries)
                    break;

            } while (IsTooClose(pos, usedPositions, minDistance));

            usedPositions.Add(pos);

            GameObject obj = Instantiate(macaronPrefab, pos, Quaternion.identity, this.transform);

            Macaron macaron = obj.GetComponent<Macaron>();
            macaron.index = i;

            if (useRandomSprites)
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = shuffledSprites[i];
                }
            }
        }
    }

    bool IsTooClose(Vector2 pos, List<Vector2> existing, float minDist)
    {
        foreach (Vector2 e in existing)
        {
            if (Vector2.Distance(pos, e) < minDist)
                return true;
        }
        return false;
    }
}
