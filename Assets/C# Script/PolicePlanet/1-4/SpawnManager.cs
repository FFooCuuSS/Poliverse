using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private List<GameObject> hatPrefabs;
    [SerializeField] private List<GameObject> glassesPrefabs;
    [SerializeField] private List<GameObject> mustachePrefabs;
    [SerializeField] private List<GameObject> montagePrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform hatSpawnPoint;
    [SerializeField] private Transform glassesSpawnPoint;
    [SerializeField] private Transform mustacheSpawnPoint;
    [SerializeField] private Transform montageSpawnPoint;

    [Header("Refs")]
    [SerializeField] private AccessoryBlinkManager blinkManager;
    [SerializeField] private Minigame_1_4 minigame;

    private readonly List<Accessory> accessories = new List<Accessory>();
    private GameObject selectedMontage;

    private void Start()
    {
        SpawnNewRound();
    }

    public void SpawnNewRound()
    {
        SpawnNewRoundImmediate();

        if (minigame != null)
            minigame.SetAccessoryOrder(new List<Accessory>(accessories));
    }

    public IEnumerator SpawnNewRoundWithFade(float fadeDuration)
    {
        SpawnNewRoundImmediate();

        // 처음엔 전부 투명하게
        SetObjectAlpha(selectedMontage, 0f);

        foreach (var acc in accessories)
        {
            if (acc != null)
                SetObjectAlpha(acc.gameObject, 0f);
        }

        // 미니게임에 새 라운드 전달
        if (minigame != null)
            minigame.SetAccessoryOrder(new List<Accessory>(accessories));

        // 0.2초 페이드 인
        FadeObject(selectedMontage, 1f, fadeDuration);

        foreach (var acc in accessories)
        {
            if (acc != null)
                FadeObject(acc.gameObject, 1f, fadeDuration);
        }

        yield return new WaitForSeconds(fadeDuration);
    }

    public void DespawnCurrentRoundObjects()
    {
        ClearCurrentObjectsImmediate();
    }

    public IEnumerator DespawnCurrentRoundObjectsWithFade(float fadeDuration)
    {
        FadeObject(selectedMontage, 0f, fadeDuration);

        foreach (var acc in accessories)
        {
            if (acc != null)
                FadeObject(acc.gameObject, 0f, fadeDuration);
        }

        yield return new WaitForSeconds(fadeDuration);

        ClearCurrentObjectsImmediate();
    }

    private void SpawnNewRoundImmediate()
    {
        ClearCurrentObjectsImmediate();

        accessories.Clear();

        if (blinkManager != null)
            blinkManager.ClearAccessories();

        SpawnAccessory(hatPrefabs, hatSpawnPoint);
        SpawnAccessory(glassesPrefabs, glassesSpawnPoint);
        SpawnAccessory(mustachePrefabs, mustacheSpawnPoint);

        SpawnMontage();
    }

    private void SpawnAccessory(List<GameObject> prefabs, Transform point)
    {
        if (prefabs == null || prefabs.Count == 0 || point == null)
            return;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];

        GameObject selected = Instantiate(prefab, point);
        selected.transform.localPosition = Vector3.zero;
        selected.transform.localRotation = Quaternion.identity;
        selected.transform.localScale = Vector3.one;

        Accessory acc = selected.GetComponent<Accessory>();
        if (acc == null)
        {
            Debug.LogWarning($"[SpawnManager] Accessory component missing on {selected.name}");
            Destroy(selected);
            return;
        }

        accessories.Add(acc);

        if (blinkManager != null)
            blinkManager.RegisterAccessory(acc);
    }

    private void SpawnMontage()
    {
        if (montagePrefabs == null || montagePrefabs.Count == 0 || montageSpawnPoint == null)
            return;

        selectedMontage = Instantiate(
            montagePrefabs[Random.Range(0, montagePrefabs.Count)],
            montageSpawnPoint.position,
            montageSpawnPoint.rotation,
            montageSpawnPoint
        );

        Montage montage = selectedMontage.GetComponent<Montage>();

        foreach (var acc in accessories)
        {
            if (acc != null)
                acc.SetMontage(montage);
        }
    }

    private void ClearCurrentObjectsImmediate()
    {
        foreach (var acc in accessories)
        {
            if (acc != null)
                Destroy(acc.gameObject);
        }

        accessories.Clear();

        if (selectedMontage != null)
        {
            Destroy(selectedMontage);
            selectedMontage = null;
        }

        if (blinkManager != null)
            blinkManager.ClearAccessories();
    }

    private void FadeObject(GameObject target, float endAlpha, float duration)
    {
        if (target == null) return;

        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            sr.DOFade(endAlpha, duration);
        }
    }

    private void SetObjectAlpha(GameObject target, float alpha)
    {
        if (target == null) return;

        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}