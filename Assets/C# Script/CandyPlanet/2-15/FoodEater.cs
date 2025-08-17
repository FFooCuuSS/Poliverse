using UnityEngine;
using UnityEngine.Events;

public class DonutEater : MonoBehaviour
{
    private Minigame_2_15 minigame_2_15;
    public GameObject stage_2_15;

    [Header("���� ����")]
    public Transform donutTransform;       // ���� ȸ�� �߽�
    public Material donutMaterial;         // ���� Shader ��Ƽ����
    public int sliceCount = 12;
    public float rotationSpeed = 30f;
    public float donutRadius = 1.5f;        // ���� �߽ɿ��� �������� �Ÿ�

    [Header("�Դ� ���� ����")]
    public Transform eatZoneTransform;      // �Դ� ���� ������Ʈ (ĸ�� ���)
    public Rect eatZoneRect = new Rect(-0.75f, -0.5f, 1.5f, 1f); // ���� Rect ����


    private int eatenFlags = 0; // ��Ʈ ����ũ�� ���� ���� ���� ����


    private void Start()
    {
        minigame_2_15 = stage_2_15.GetComponent<Minigame_2_15>();
    }
    void Update()
    {
        // 1. ���� ȸ��
        donutTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 2. Shader�� ȸ��/���� ���� ����
        float currentRotation = donutTransform.eulerAngles.z;
        donutMaterial.SetFloat("_Rotation", currentRotation);
        donutMaterial.SetFloat("_SliceCount", sliceCount);
        donutMaterial.SetVector("_EatenFlags", new Vector4(eatenFlags, 0, 0, 0));

        // 3. Ŭ�� ó��
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
            // 1. �� ���� �߽� ����
            float angle = i * sliceAngle + sliceAngle / 2f;
            float worldAngle = (angle + donutRotation) * Mathf.Deg2Rad;

            // 2. ���� ���ؿ��� ���� �߽� ��ǥ ���
            Vector2 localOffset = new Vector2(
                Mathf.Cos(worldAngle),
                Mathf.Sin(worldAngle)
            ) * donutRadius;

            Vector2 worldPos = (Vector2)donutTransform.position + localOffset;

            // 3. �Դ� ���� �������� local ��ǥ ��ȯ
            Vector2 localToEatZone = Quaternion.Inverse(eatZoneTransform.rotation) *
                                     (worldPos - (Vector2)eatZoneTransform.position);

            // 4. Rect �ȿ� ������ ����
            if (eatZoneRect.Contains(localToEatZone))
            {
                int mask = 1 << i;
                if ((eatenFlags & mask) == 0)
                {
                    eatenFlags |= mask;
                    newSlicesEaten++;
                    Debug.Log($"���� {i} ����");
                }
            }
        }

        if (newSlicesEaten > 0 && CheckClear())
        {
            Debug.Log(" Ŭ����!");
            minigame_2_15.Succeed();

        }
    }

    bool CheckClear()
    {
        int fullMask = (1 << sliceCount) - 1;
        return (eatenFlags & fullMask) == fullMask;
    }
}


