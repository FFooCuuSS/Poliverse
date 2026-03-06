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

    private void Start()
    {
        minigame_2_7 = GetComponentInParent<Minigame_2_7>();
        Spawn();
    }
    public void Spawn()
    {
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
        jelly.Init(this);
        stone.SetJelly(obj.transform);

        currentIndex++;
    }
}
