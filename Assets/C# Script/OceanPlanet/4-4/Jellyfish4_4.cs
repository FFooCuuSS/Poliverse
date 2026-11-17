using UnityEngine;

public class Jellyfish4_4 : MonoBehaviour
{
    public Transform firePoint;       // 전기가 나가는 위치(해파리 왼쪽 앞)
    public GameObject boltPrefab;     // 투사체 전기

    public void Fire(Transform boltParent)
    {
        if (boltPrefab == null || firePoint == null) return;
        Instantiate(boltPrefab, firePoint.position, Quaternion.identity, boltParent);
    }
}