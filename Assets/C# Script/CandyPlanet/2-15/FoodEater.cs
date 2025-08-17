using UnityEngine;
using UnityEngine.Events;

public class DonutEater : MonoBehaviour
{
    private Minigame_2_15 minigame_2_15;
    public GameObject stage_2_15;

    [Header("도넛 설정")]
    public Transform donutTransform;       // 도넛 회전 중심
    public Material donutMaterial;         // 도넛 Shader 머티리얼
    public int sliceCount = 12;
    public float rotationSpeed = 30f;
    public float donutRadius = 1.5f;        // 도넛 중심에서 조각까지 거리

    [Header("먹는 구역 설정")]
    public Transform eatZoneTransform;      // 먹는 구역 오브젝트 (캡슐 모양)
    public Rect eatZoneRect = new Rect(-0.75f, -0.5f, 1.5f, 1f); // 로컬 Rect 영역


    private int eatenFlags = 0; // 비트 마스크로 조각 먹은 상태 저장


    private void Start()
    {
        minigame_2_15 = stage_2_15.GetComponent<Minigame_2_15>();
    }
    void Update()
    {
        // 1. 도넛 회전
        donutTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 2. Shader에 회전/조각 정보 전달
        float currentRotation = donutTransform.eulerAngles.z;
        donutMaterial.SetFloat("_Rotation", currentRotation);
        donutMaterial.SetFloat("_SliceCount", sliceCount);
        donutMaterial.SetVector("_EatenFlags", new Vector4(eatenFlags, 0, 0, 0));

        // 3. 클릭 처리
        if (Input.GetMouseButtonDown(0))
        {
            TryEatSlicesByRect(currentRotation);
        }
    }

    void TryEatSlicesByRect(float donutRotation)
    {
        float sliceAngle = 360f / sliceCount;
        int newSlicesEaten = 0;

        for (int i = 0; i < sliceCount; i++)
        {
            // 1. 각 조각 중심 각도
            float angle = i * sliceAngle + sliceAngle / 2f;
            float worldAngle = (angle + donutRotation) * Mathf.Deg2Rad;

            // 2. 도넛 기준에서 조각 중심 좌표 계산
            Vector2 localOffset = new Vector2(
                Mathf.Cos(worldAngle),
                Mathf.Sin(worldAngle)
            ) * donutRadius;

            Vector2 worldPos = (Vector2)donutTransform.position + localOffset;

            // 3. 먹는 구역 기준으로 local 좌표 변환
            Vector2 localToEatZone = Quaternion.Inverse(eatZoneTransform.rotation) *
                                     (worldPos - (Vector2)eatZoneTransform.position);

            // 4. Rect 안에 들어오면 먹힘
            if (eatZoneRect.Contains(localToEatZone))
            {
                int mask = 1 << i;
                if ((eatenFlags & mask) == 0)
                {
                    eatenFlags |= mask;
                    newSlicesEaten++;
                    Debug.Log($"조각 {i} 먹힘");
                }
            }
        }

        if (newSlicesEaten > 0 && CheckClear())
        {
            Debug.Log(" 클리어!");
            minigame_2_15.Succeed();

        }
    }

    bool CheckClear()
    {
        int fullMask = (1 << sliceCount) - 1;
        return (eatenFlags & fullMask) == fullMask;
    }
}


