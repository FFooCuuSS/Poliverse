using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompletedMacaroon : MonoBehaviour
{
    [Header("정답 마카롱 프리팹")]
    public GameObject macaronPrefab;

    [Header("사용할 마카롱 스프라이트들")]
    public Sprite[] macaronSprites;

    [Header("탑 위치 설정")]
    public float yOffset = 0.4f;

    [HideInInspector]
    public int[] answerOrder = new int[5];

    [Header("옵션")]
    [SerializeField] private bool useRandomSprites = false; // 리소스 준비되면 true

    void Start()
    {
        if (macaronSprites.Length == 0)
            return;

        GenerateAnswerTower();
    }

    void GenerateAnswerTower()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < macaronSprites.Length; i++)
            indices.Add(i);

        for (int i = 0; i < indices.Count; i++)
        {
            int rand = Random.Range(i, indices.Count);
            (indices[i], indices[rand]) = (indices[rand], indices[i]);
        }

        for (int i = 0; i < 5; i++)
        {
            int idx = indices[i];
            answerOrder[i] = idx;

            GameObject obj = Instantiate(macaronPrefab, transform);

            Macaron m = obj.GetComponent<Macaron>();
            m.index = idx;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (useRandomSprites)
                {
                    sr.sprite = macaronSprites[indices[i]];
                }
                else
                {
                    sr.sprite = macaronSprites[idx];
                }

                sr.sortingOrder = i + 1;
            }

            obj.transform.localPosition = new Vector3(0, yOffset * i, 0);

            if (obj.TryGetComponent<DragAndDrop>(out var drag))
                drag.enabled = false;
        }
    }
}
