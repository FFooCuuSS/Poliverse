using UnityEngine;

public class FoodRotate : MonoBehaviour
{
    [Header("회전 속도 (양수 = 시계 방향, 음수 = 반시계)")]
    public float rotateSpeed = 60f;

    [Header("속도 랜덤 범위 적용 여부")]
    public bool useRandomSpeed = false;
    public float minSpeed = 40f;
    public float maxSpeed = 100f;

    private void Start()
    {
        if (useRandomSpeed)
        {
            rotateSpeed = Random.Range(minSpeed, maxSpeed) * (Random.value > 0.5f ? 1 : -1);
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }
}
