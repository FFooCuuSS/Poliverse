using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacaroonSpawn : MonoBehaviour
{
    [Header("Prefab & Sprites")]
    public GameObject macaronPrefab;
    public Sprite[] macaronSprites;

    [Header("Line Spawn Settings")]
    public float startX = -3f;
    public float startY = 2f;
    public float spacing = 2.5f; // 마카롱 사이 간격

    [Header("Options")]
    [SerializeField] private bool useRandomSprites = false; //리소스 받으면 true변경

    void Start()
    {
        if (macaronSprites == null || macaronSprites.Length == 0)
        {
            Debug.LogWarning("Macaron sprites not assigned!");
            return;
        }

        int macaronCount = macaronSprites.Length;

        // 0 ~ n-1 인덱스 리스트 생성
        List<int> indices = new List<int>();
        for (int i = 0; i < macaronCount; i++)
            indices.Add(i);

        // 랜덤 섞기
        for (int i = 0; i < indices.Count; i++)
        {
            int rand = Random.Range(i, indices.Count);
            int temp = indices[i];
            indices[i] = indices[rand];
            indices[rand] = temp;
        }

        float totalWidth = (macaronCount - 1) * spacing;
        float offsetX = -totalWidth / 2f;

        for (int i = 0; i < macaronCount; i++)
        {
            Vector2 pos = new Vector2(
                startX + offsetX + (i * spacing),
                startY
            );

            GameObject obj = Instantiate(macaronPrefab, pos, Quaternion.identity, this.transform);

            Macaron macaron = obj.GetComponent<Macaron>();
            macaron.index = indices[i];

            //이미지 비율 유지하며 크기 조정
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // 기준이 될 프리팹 원래 크기
                SpriteRenderer prefabSR = macaronPrefab.GetComponent<SpriteRenderer>();
                Vector2 targetSize = prefabSR.bounds.size;

                // 스프라이트 교체
                sr.sprite = macaronSprites[indices[i]];

                // 새 스프라이트 실제 크기
                Vector2 spriteSize = sr.sprite.bounds.size;

                // 비율 계산
                float scaleX = targetSize.x / spriteSize.x;
                float scaleY = targetSize.y / spriteSize.y;

                float finalScale = Mathf.Min(scaleX, scaleY); // 비율 유지

                obj.transform.localScale = new Vector3(finalScale, finalScale, 1f);
            }

        }
    }

}
