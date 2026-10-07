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

    [SerializeField]
    public float centerOffsetX = -2f;

    private readonly List<GameObject>
        spawnedMacarons =
        new List<GameObject>();

    public List<GameObject> GetSpawnedMacarons()
    {
        return spawnedMacarons;
    }

    public int SpawnMacarons(
        IReadOnlyList<int> pattern = null)
    {
        int slotCount =
            macaronSprites.Length;

        IReadOnlyList<int> activeSlots =
            pattern ??
            DefaultAllSlots(slotCount);

        SpriteRenderer prefabSR =
            macaronPrefab.GetComponent<SpriteRenderer>();

        Vector2 targetSize =
            prefabSR.bounds.size;

        float totalWidth =
            (slotCount - 1) * spacing;

        float startPosX =
            -totalWidth / 2f +
            centerOffsetX;

        spawnedMacarons.Clear();

        for (int i = 0;
             i < activeSlots.Count;
             i++)
        {
            int slot =
                activeSlots[i];

            if (slot < 0 ||
                slot >= slotCount)
            {
                Debug.LogWarning(
                    $"[MacaroonSpawn] " +
                    $"Àß¸øµÈ ½½·Ô ÀÎµ¦½º: {slot}"
                );

                continue;
            }

            Vector2 pos =
                new Vector2(
                    startPosX +
                    slot * spacing,
                    startY
                );

            GameObject obj =
                Instantiate(
                    macaronPrefab,
                    pos,
                    Quaternion.identity,
                    transform
                );

            Macaron macaron =
                obj.GetComponent<Macaron>();

            if (macaron != null)
            {
                macaron.index = slot;
                macaron.isStacked = false;
            }

            SpriteRenderer sr =
                obj.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.sprite =
                    macaronSprites[slot];

                Vector2 spriteSize =
                    sr.sprite.bounds.size;

                float scaleX =
                    targetSize.x /
                    spriteSize.x;

                float scaleY =
                    targetSize.y /
                    spriteSize.y;

                float finalScale =
                    Mathf.Min(
                        scaleX,
                        scaleY
                    );

                obj.transform.localScale =
                    new Vector3(
                        finalScale,
                        finalScale,
                        1f
                    );
            }

            spawnedMacarons.Add(obj);
        }

        return spawnedMacarons.Count;
    }


    public void ClearUncollectedMacarons()
    {
        for (int i = 0;
             i < spawnedMacarons.Count;
             i++)
        {
            GameObject obj =
                spawnedMacarons[i];

            if (obj == null)
                continue;

            Macaron m =
                obj.GetComponent<Macaron>();

            if (m != null &&
                m.isStacked)
            {
                continue;
            }

            Destroy(obj);
        }

        spawnedMacarons.Clear();
    }


    private static List<int> DefaultAllSlots(
        int count)
    {
        List<int> list =
            new List<int>(count);

        for (int i = 0;
             i < count;
             i++)
        {
            list.Add(i);
        }

        return list;
    }
}