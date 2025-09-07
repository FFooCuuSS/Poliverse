using System.Collections.Generic;
using UnityEngine;

public class GameManager2_4 : MonoBehaviour
{
    public static GameManager2_4 Instance;
    public MiniGame2_4 miniGame2_4;

    [Header("Bottle Settings")]
    public GameObject bottlePrefab;
    public int bottleCount = 10;  // ������ �� ����
    public int capacity = 4;      // �� �� �뷮
    public Transform bottleParent;

    [Header("Colors")]
    public Color[] possibleColors;

    public List<Bottle2_4> bottles = new List<Bottle2_4>();

    private void Awake() => Instance = this;

    private void Start() => SpawnBottles();


    void SpawnBottles()
    {
        bottles.Clear();
        int cap = capacity; // 4ĭ

        if (bottleCount != possibleColors.Length)
        {
            Debug.LogError("�� ���� ���� ���� ���� �ʽ��ϴ�!");
            return;
        }

        // 1. �� �� ���� ����
        List<Color> topColors = new List<Color>(possibleColors);

        // 2. �߰� ���� Ǯ ���� (����� 2����)
        List<Color> middleColorsPool = new List<Color>();
        foreach (var c in possibleColors)
        {
            middleColorsPool.Add(c);
            middleColorsPool.Add(c);
        }

        // 3. �߰� ���� ����
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

        // 4. �� ����
        for (int i = 0; i < bottleCount; i++)
        {
            Vector3 pos = new Vector3(startX + (i % 5) * gap, -(i / 5) * 3f, 0);
            GameObject obj = Instantiate(bottlePrefab, pos, Quaternion.identity, bottleParent);
            Bottle2_4 bottle = obj.GetComponent<Bottle2_4>();
            bottle.capacity = cap;
            bottle.liquids.Clear();

            List<Color> tempColors = new List<Color>();

            // �� �Ʒ� �� ����
            tempColors.Add(Color.clear);

            // �߰� 2ĭ �� ������� �Ҵ�
            tempColors.Add(middleColorsPool[middleIndex++]);
            tempColors.Add(middleColorsPool[middleIndex++]);

            // �� �� �� ���� ����
            tempColors.Add(topColors[i]);

            // Stack�� Push �� �� �Ʒ� �� �߰� �� �� ��
            for (int j = tempColors.Count - 1; j >= 0; j--)
                bottle.liquids.Push(tempColors[j]);

            // �ð�ȭ
            bottle.UpdateVisual();
            bottles.Add(bottle);
        }
    }



    public Color GetRandomColor()
    {
        if (possibleColors == null || possibleColors.Length == 0)
        {
            Debug.LogError(" possibleColors �迭�� ����ֽ��ϴ�!");
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

        Debug.Log("2-4 ����!");
        miniGame2_4.Success();
    }
}
