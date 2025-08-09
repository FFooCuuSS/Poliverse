using System.Collections;
using UnityEngine;

public class Phone_3_6 : MonoBehaviour
{
    private bool isImportant;
    private Transform ContentPos;

    public GameObject ImportantContent;
    public GameObject NormalContent;

    private Coroutine callRoutine;

    private void Start()
    {
        ContentPos = transform.GetChild(0); // ��ǥ ����
    }

    public void PhoneCalling()
    {
        if (callRoutine == null)
        {
            callRoutine = StartCoroutine(CallRoutine());
        }
    }

    private IEnumerator CallRoutine()
    {
        // ù ��° ���� ����
        isImportant = Random.value > 0.5f;
        yield return StartCoroutine(SpawnAndWait(isImportant));

        // ���ĺ��ʹ� �����ư��� �ݺ�
        while (true)
        {
            isImportant = !isImportant;
            yield return StartCoroutine(SpawnAndWait(isImportant));
        }
    }

    private IEnumerator SpawnAndWait(bool showImportant)
    {
        GameObject prefabToSpawn = showImportant ? ImportantContent : NormalContent;

        float duration = Random.Range(1f, 3f);
        GameObject spawned = Instantiate(prefabToSpawn, ContentPos.position, Quaternion.identity);

        yield return new WaitForSeconds(duration);

        Destroy(spawned);
    }

    public bool BoolCheck()
    {
        return isImportant;
    }
}
