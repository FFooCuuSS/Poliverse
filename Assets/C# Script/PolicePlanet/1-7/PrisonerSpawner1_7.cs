using UnityEngine;

public class PrisonerSpawner1_7 : MonoBehaviour
{
    
    public GameObject[] prisonerPrefabs;
    public Transform spawnPoint;
    public bool isSpawning = true;

    public GameObject SpawnRandomPrisoner()
    {
        int randomIndex = Random.Range(0, prisonerPrefabs.Length);

        GameObject prisoner = Instantiate(prisonerPrefabs[randomIndex], spawnPoint.position, Quaternion.identity, this.transform);
        return prisoner;  // ��ȯ �߰�
    }

    /*
    private void Update()
    {
        if(isSpawning)
        {
            GameManager1_7.instance.SpawnPrisonerAndItems();
            isSpawning = false;
        }
        if (transform.childCount == 0)
        {
            isSpawning = true;
            Debug.Log("�ڽ��� �����Ƿ� isSpawning�� true�� �����߽��ϴ�.");
        }
    }
    */
}
