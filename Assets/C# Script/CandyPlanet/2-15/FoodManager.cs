using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public List<GameObject> foodPrefabs;
    public Transform spawnPoint;

    private int currentIndex = 0;
    public static FoodManager Instance;
    private Minigame_2_15 minigame_2_15;
    public GameObject stage_2_15;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnNextFood();
        minigame_2_15 = stage_2_15.GetComponent<Minigame_2_15>();

    }

    public void SpawnNextFood()
    {
        if (currentIndex >= foodPrefabs.Count)
        {
            Debug.Log("모든 음식 끝!");
            minigame_2_15.Succeed();
            return;
        }

        // 루트 Prefab Instantiate
        Debug.Log($"Spawning food {currentIndex}, prefab = {foodPrefabs[currentIndex]} at {spawnPoint.position}");

        GameObject foodRoot = Instantiate(foodPrefabs[currentIndex], spawnPoint.position, Quaternion.identity);
        currentIndex++;

        // FoodAssembler가 붙어 있으면 조각 생성
        FoodAssembler assembler = foodRoot.GetComponent<FoodAssembler>();
        if (assembler != null)
        {
            assembler.AssembleSlices(foodRoot.transform);
        }
        else
        {
            Debug.LogWarning("FoodAssembler 스크립트가 Prefab에 없음!");
        }
    }
}
