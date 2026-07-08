using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacaroonSpawn : MonoBehaviour
{
    public GameObject macaronPrefab;
    public Sprite[] macaronSprites;

    public float startX = -3f;
    public float startY = 2f;
    public float spacing = 2.5f;

    [SerializeField] public float centerOffsetX = -2f;

    public void SpawnMacarons()
    {
        int macaronCount = macaronSprites.Length;

        SpriteRenderer prefabSR = macaronPrefab.GetComponent<SpriteRenderer>();
        Vector2 targetSize = prefabSR.bounds.size;

        float totalWidth = (macaronCount - 1) * spacing;

        // ±‚¡∏ ¡ﬂæ” ±‚¡ÿ + øﬁ¬  ¿Ãµø
        float startPosX = -totalWidth / 2f + centerOffsetX;


        for (int i = 0; i < macaronCount; i++)
        {
            Vector2 pos = new Vector2(
                startPosX + i * spacing,
                startY
            );


            GameObject obj = Instantiate(
                macaronPrefab,
                pos,
                Quaternion.identity,
                transform
            );


            Macaron macaron = obj.GetComponent<Macaron>();
            macaron.index = i;


            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            sr.sprite = macaronSprites[i];


            Vector2 spriteSize = sr.sprite.bounds.size;

            float scaleX = targetSize.x / spriteSize.x;
            float scaleY = targetSize.y / spriteSize.y;

            float finalScale = Mathf.Min(scaleX, scaleY);

            obj.transform.localScale =
                new Vector3(finalScale, finalScale, 1f);
        }
    }
}
