using UnityEngine;

public class DecorationLoop_1_3 : MonoBehaviour
{
    [Header("Spawn Rule")]
    [SerializeField] private float spawnTriggerX = 0f;   // 이 값보다 작아지면 생성
    [SerializeField] private float spawnOffsetX = 16f;   // 뒤에 붙일 간격(너 요구: 16f)

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.5f;     // 왼쪽 이동 속도(조절)

    [Header("Cleanup")]
    [SerializeField] private float destroyX = -40f;      // 너무 왼쪽 가면 제거(조절)

    private bool spawnedNext = false;

    private void Update()
    {
        // 1) 자동 왼쪽 이동
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);

        // 2) 트리거 지나가면 "뒤에" 복제 생성(1회)
        if (!spawnedNext && transform.position.x < spawnTriggerX)
        {
            SpawnNext();
            spawnedNext = true;
        }

        // 3) 너무 멀리 가면 삭제
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }

    private void SpawnNext()
    {
        Vector3 pos = transform.position;
        pos.x += spawnOffsetX;

        GameObject clone = Instantiate(gameObject, pos, transform.rotation, transform.parent);

        // 복제본은 "이 프레임에 바로 또 생성"하면 폭주할 수 있으니 방지
        var loop = clone.GetComponent<DecorationLoop_1_3>();
        if (loop != null)
        {
            loop.spawnedNext = false;  // 다음 조각은 아직 생성 안 했으니 false
        }
    }
}
