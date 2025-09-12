using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonutEater : MonoBehaviour
{
    private DountManager manager;
    private Material donutMaterial;

    [Header("조각 설정")]
    public int sliceCount = 12;
    public float radius = 1.5f;

    [Header("12시 방향 먹는 범위")]
    public float eatAngle = 30f; // ±eatAngle/2 범위 내 조각들이 먹힘

    private int eatenFlags = 0;

    // 초기화: 매니저 연결
    public void Init(DountManager mgr)
    {
        manager = mgr;
    }

    private void Start()
    {
        donutMaterial = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        // 머티리얼에 조각 상태 전달
        donutMaterial.SetFloat("_Rotation", transform.eulerAngles.z);
        donutMaterial.SetFloat("_SliceCount", sliceCount);
        donutMaterial.SetVector("_EatenFlags", new Vector4(eatenFlags, 0, 0, 0));

        // 클릭 시 12시 근처 조각 먹기 처리
        if (Input.GetMouseButtonDown(0))
        {
            TryEatSlicesNearTop(transform.eulerAngles.z);
        }
    }

    // 12시 방향 근처 조각들 먹기
    void TryEatSlicesNearTop(float donutRotation)
    {
        float sliceAngle = 360f / sliceCount;
        int newSlicesEaten = 0;

        // 12시 방향 기준 각도 (Unity 상에서 0도 = 오른쪽, 90도 = 위)
        float topAngle = (90f - donutRotation + 360f) % 360f;

        for (int i = 0; i < sliceCount; i++)
        {
            float angle = (i * sliceAngle + sliceAngle / 2f) % 360f;

            // ±eatAngle/2 범위 안이면 먹힘
            float diff = Mathf.DeltaAngle(topAngle, angle);
            if (Mathf.Abs(diff) <= eatAngle / 2f)
            {
                int mask = 1 << i;
                if ((eatenFlags & mask) == 0)
                {
                    eatenFlags |= mask;
                    newSlicesEaten++;
                    Debug.Log($"조각 {i} (12시 근처) 먹힘");
                }
            }
        }

        // 도넛 모든 조각 먹으면 매니저에 알림
        if (newSlicesEaten > 0 && CheckClear())
        {
            manager.OnDonutCleared();
        }
    }

    // 도넛 모든 조각 먹힘 체크
    bool CheckClear()
    {
        int fullMask = (1 << sliceCount) - 1;
        return (eatenFlags & fullMask) == fullMask;
    }
}
