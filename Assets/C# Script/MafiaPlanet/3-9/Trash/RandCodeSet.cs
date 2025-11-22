using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandCodeSet : MonoBehaviour
{
    public Transform[] setPositions;
    public GameObject[] codes;
    
    public List<Transform> answerPosList;
    public List<GameObject> answerCodeList = new List<GameObject>();
    public List<GameObject> prefabs; // 프리팹 리스트
    // Start is called before the first frame update
    void Start()
    {
        int[] randomOrder = GetRandomOrder(setPositions.Length);
        for (int i = 0; i < codes.Length && i < setPositions.Length; i++)
        {
            Instantiate(codes[i], setPositions[randomOrder[i]].position, Quaternion.identity);
            codes[i].name = "Code" + (i + 1);
        }
    }
    public void SpawnNext()
    {
        // 예외 처리
        if (answerCodeList.Count >= answerPosList.Count)
        {
            Debug.LogWarning("모든 위치가 이미 사용됨!");
            return;
        }
        if (answerCodeList.Count >= prefabs.Count)
        {
            Debug.LogWarning("모든 프리팹을 이미 생성함!");
            return;
        }

        // 생성할 인덱스 (현재 생성된 개수와 동일)
        int index = answerCodeList.Count;

        // 지정된 위치와 프리팹 가져오기
        Transform pos = answerPosList[index];
        GameObject prefab = prefabs[index];

        // Instantiate
        GameObject obj = Instantiate(prefab, pos.position, pos.rotation);
        obj.name = prefab.name + "_Spawned" + index;

        // 리스트에 기록 (나중에 Destroy용)
        answerCodeList.Add(obj);

        Debug.Log($"[AnswerSpawner] {index}번 생성 완료 → {pos.name} 위치에 {prefab.name}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private int[] GetRandomOrder(int n)
    {
        int[] order = new int[n];
        for (int i=0;i<n;i++)
        {
            order[i] = i;
        }
        for(int i=0;i<n;i++)
        {
            int rand = Random.Range(i,n);
            int temp = order[i];
            order[i] = order[rand];
            order[rand] = temp;
        }
        return order;
    }
}
