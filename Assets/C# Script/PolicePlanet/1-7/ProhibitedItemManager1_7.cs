using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemManager1_7 : MonoBehaviour
{
    public GameObject stage_1_7;
    private Minigame_1_7 minigame_1_7;

    [Header("���� ��ǰ ��������Ʈ ����Ʈ")]
    [SerializeField] public List<Sprite> prohibitedSprites;

    [Header("���� ��ũ ��������Ʈ ����Ʈ")]
    [SerializeField] private List<Sprite> prohibitedMarkSprites;

    [Header("�浹 ī��Ʈ ����Ʈ")]
    [SerializeField] private List<int> collectedCounts;

    [Header("���� ��ũ ������Ʈ")]
    [SerializeField] private SpriteRenderer prohibitedMarkRenderer;

    [HideInInspector] public int targetIndex = -1;

    [HideInInspector] public int[] prefabCounts; // �� ��������Ʈ�� ������ ���� ����

    void Start()
    {
        collectedCounts = new List<int>(new int[prohibitedSprites.Count]);
        minigame_1_7 = stage_1_7.GetComponent<Minigame_1_7>();

        //// ���� ��ũ�� ���� ����
        //targetIndex = Random.Range(0, prohibitedMarkSprites.Count);
        //prohibitedMarkRenderer.sprite = prohibitedMarkSprites[targetIndex];
    }

    // �浹 �� ȣ��� �Լ�
    public void RegisterCollision(Sprite collidedSprite)
    {
        int index = prohibitedSprites.IndexOf(collidedSprite);
        if (index != -1)
        {
            collectedCounts[index]++;
            
            Debug.Log("Count++");
        }
    }

    public void SetPrefabCounts(int[] counts)
    {
        prefabCounts = counts;

        // ���� ��ũ�� ���� ������ ��������Ʈ �ε��� ���͸� (1�� �̻� ������ �͸�)
        List<int> validIndices = new List<int>();
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] > 0)
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("������ ������ �� ���� ��ǰ���� ����� �� �ִ� ��������Ʈ�� �����ϴ�.");
            return;
        }

        // ��ȿ�� �ε��� �߿��� ���� ��ũ�� ���� ����
        targetIndex = validIndices[Random.Range(0, validIndices.Count)];
        prohibitedMarkRenderer.sprite = prohibitedMarkSprites[targetIndex];
    }

    public void UpdateZoneCount(int[] currentZoneCounts)
    {
        if (prefabCounts == null || targetIndex < 0 || targetIndex >= currentZoneCounts.Length)
            return;

        if (currentZoneCounts[targetIndex] == prefabCounts[targetIndex])
        {
            minigame_1_7.Success();
        }
    }
}
