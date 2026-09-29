using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class weaponSpawner_3_15 : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("마법봉 포함 전부. 마법봉 여부는 각 프리팹의 weapon_3_15.isMagicWand로 판단")]
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField, Range(0f, 1f)] private float magicWandChance = 0.5f;

    [Header("Refs")]
    [FormerlySerializedAs("manager_3_15")]
    [SerializeField] private GameObject managerObj;
    [Tooltip("비우면 스포너의 부모 아래에 생성")]
    [SerializeField] private Transform spawnParent;

    [Header("Spawn")]
    [SerializeField] private float xMoving;
    [SerializeField] private float spawnInterval = 1f;

    [System.NonSerialized] public bool banMoving = true;

    private Vector2 spawnPos;
    private float elapsed;
    private manager_3_15 mgr;

    private readonly List<GameObject> magicPrefabs = new();
    private readonly List<GameObject> normalPrefabs = new();

    private void Start()
    {
        spawnPos = transform.GetChild(0).position;
        elapsed = spawnInterval; // 시작하자마자 1개

        mgr = managerObj != null ? managerObj.GetComponent<manager_3_15>() : null;
        if (spawnParent == null) spawnParent = transform.parent;

        // 리스트를 isMagicWand 기준으로 분류
        foreach (var p in weaponPrefabs)
        {
            if (p == null) continue;
            if (!p.TryGetComponent(out weapon_3_15 w))
            {
                Debug.LogWarning($"[weaponSpawner_3_15] {p.name}에 weapon_3_15 없음");
                continue;
            }
            (w.IsMagicWand ? magicPrefabs : normalPrefabs).Add(p);
        }

        Debug.Log($"[weaponSpawner_3_15] {name} → 마법봉 {magicPrefabs.Count}개: [{string.Join(", ", magicPrefabs.ConvertAll(x => x.name))}] / " +
                  $"일반 {normalPrefabs.Count}개: [{string.Join(", ", normalPrefabs.ConvertAll(x => x.name))}] / 마법봉 확률 {magicWandChance:P0}");

        if (normalPrefabs.Count == 0)
            Debug.LogWarning($"[weaponSpawner_3_15] {name}: 일반 무기가 없어서 마법봉만 스폰됨 (프리팹 isMagicWand 체크 확인)");

        if (magicPrefabs.Count == 0)
            Debug.LogWarning($"[weaponSpawner_3_15] {name}: isMagicWand 켜진 프리팹이 리스트에 없음");
    }

    private void Update()
    {
        if (banMoving) return;

        elapsed += Time.deltaTime;
        if (elapsed < spawnInterval) return;
        elapsed = 0f;

        bool magic = magicPrefabs.Count > 0 &&
                     (normalPrefabs.Count == 0 || Random.value < magicWandChance);
        var pool = magic ? magicPrefabs : normalPrefabs;
        if (pool.Count == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Count)];
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, spawnParent);
        obj.GetComponent<weapon_3_15>().Init(mgr, xMoving);
    }
}