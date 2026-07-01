using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellySpawner : MonoBehaviour
{
    public JellyType[] jellyTypes;
    [SerializeField] private Stone stone;
    private Minigame_2_7 minigame_2_7;
    public GameObject jellyParent;
    int currentIndex = 0;

    private Jelly currentJelly; // 화면엔 항상 이 하나만 존재해야 함

    private void Start()
    {
        minigame_2_7 = GetComponentInParent<Minigame_2_7>();
        Spawn();
    }
    public void Spawn()
    {
        // 새 젤리를 만들기 전에 이전 젤리를 정리 (화면엔 하나만 유지)
        if (currentJelly != null)
        {
            Destroy(currentJelly.gameObject);
            currentJelly = null;
        }

        if (currentIndex >= jellyTypes.Length)
        {
            Debug.Log("Game Success");
            minigame_2_7.Succeed();
            return;
        }

        JellyType type = jellyTypes[currentIndex];

        GameObject obj = Instantiate(
            type.jellyPrefab,
            transform.position,
            Quaternion.identity,
            jellyParent.transform
        );

        Jelly jelly = obj.GetComponent<Jelly>();
        jelly.data = type;
        jelly.Init(this, stone.GetSurfaceY());
        stone.SetJelly(jelly);

        currentJelly = jelly;
        currentIndex++;
    }
}