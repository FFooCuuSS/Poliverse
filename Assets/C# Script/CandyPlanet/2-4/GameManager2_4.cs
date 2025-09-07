using System.Collections.Generic;
using UnityEngine;

public class GameManager2_4 : MonoBehaviour
{
    public static GameManager2_4 Instance;
    public MiniGame2_4 miniGame2_4;

    [Header("Bottle Settings")]
    public GameObject bottlePrefab;
    public int bottleCount = 10;  // 생성할 병 개수
    public int capacity = 4;      // 한 병 용량
    public Transform bottleParent;

    [Header("Colors")]
    public Color[] possibleColors;

    public List<Bottle2_4> bottles = new List<Bottle2_4>();

    private void Awake() => Instance = this;

    private void Start() => SpawnBottles();


    void SpawnBottles()
    {
        bottles.Clear();
        int cap = capacity; // 4칸

        if (bottleCount != possibleColors.Length)
        {
            Debug.LogError("병 수와 색상 수가 맞지 않습니다!");
            return;
        }

        // 1. 맨 위 고유 색상
        List<Color> topColors = new List<Color>(possibleColors);

        // 2. 중간 색상 풀 생성 (색상당 2개씩)
        List<Color> middleColorsPool = new List<Color>();
        foreach (var c in possibleColors)
        {
            middleColorsPool.Add(c);
            middleColorsPool.Add(c);
        }

        // 3. 중간 색상 섞기
        for (int i = 0; i < middleColorsPool.Count; i++)
        {
            int rand = Random.Range(i, middleColorsPool.Count);
            Color temp = middleColorsPool[i];
            middleColorsPool[i] = middleColorsPool[rand];
            middleColorsPool[rand] = temp;
        }

        float startX = -4f;
        float gap = 5f;
        int middleIndex = 0;

        // 4. 병 생성
        for (int i = 0; i < bottleCount; i++)
        {
            Vector3 pos = new Vector3(startX + (i % 5) * gap, -(i / 5) * 3f, 0);
            GameObject obj = Instantiate(bottlePrefab, pos, Quaternion.identity, bottleParent);
            Bottle2_4 bottle = obj.GetComponent<Bottle2_4>();
            bottle.capacity = cap;
            bottle.liquids.Clear();

            List<Color> tempColors = new List<Color>();

            // 맨 아래 → 투명
            tempColors.Add(Color.clear);

            // 중간 2칸 → 순서대로 할당
            tempColors.Add(middleColorsPool[middleIndex++]);
            tempColors.Add(middleColorsPool[middleIndex++]);

            // 맨 위 → 고유 색상
            tempColors.Add(topColors[i]);

            // Stack에 Push → 맨 아래 → 중간 → 맨 위
            for (int j = tempColors.Count - 1; j >= 0; j--)
                bottle.liquids.Push(tempColors[j]);

            // 시각화
            bottle.UpdateVisual();
            bottles.Add(bottle);
        }
    }



    public Color GetRandomColor()
    {
        if (possibleColors == null || possibleColors.Length == 0)
        {
            Debug.LogError(" possibleColors 배열이 비어있습니다!");
            return Color.white;
        }
        return possibleColors[Random.Range(0, possibleColors.Length)];
    }

    public void CheckWin()
    {
        foreach (var bottle in bottles)
        {
            if (!bottle.IsSingleColor() && !bottle.IsEmpty())
                return;
        }

        Debug.Log("2-4 성공!");
        miniGame2_4.Success();
    }
}
