using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonutEater : MonoBehaviour
{
    private DountManager manager;
    private Material donutMaterial;

    [Header("���� ����")]
    public int sliceCount = 12;
    public float radius = 1.5f;

    [Header("12�� ���� �Դ� ����")]
    public float eatAngle = 30f; // ��eatAngle/2 ���� �� �������� ����

    private int eatenFlags = 0;

    // �ʱ�ȭ: �Ŵ��� ����
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
        // ��Ƽ���� ���� ���� ����
        donutMaterial.SetFloat("_Rotation", transform.eulerAngles.z);
        donutMaterial.SetFloat("_SliceCount", sliceCount);
        donutMaterial.SetVector("_EatenFlags", new Vector4(eatenFlags, 0, 0, 0));

        // Ŭ�� �� 12�� ��ó ���� �Ա� ó��
        if (Input.GetMouseButtonDown(0))
        {
            TryEatSlicesNearTop(transform.eulerAngles.z);
        }
    }

    // 12�� ���� ��ó ������ �Ա�
    void TryEatSlicesNearTop(float donutRotation)
    {
        float sliceAngle = 360f / sliceCount;
        int newSlicesEaten = 0;

        // 12�� ���� ���� ���� (Unity �󿡼� 0�� = ������, 90�� = ��)
        float topAngle = (90f - donutRotation + 360f) % 360f;

        for (int i = 0; i < sliceCount; i++)
        {
            float angle = (i * sliceAngle + sliceAngle / 2f) % 360f;

            // ��eatAngle/2 ���� ���̸� ����
            float diff = Mathf.DeltaAngle(topAngle, angle);
            if (Mathf.Abs(diff) <= eatAngle / 2f)
            {
                int mask = 1 << i;
                if ((eatenFlags & mask) == 0)
                {
                    eatenFlags |= mask;
                    newSlicesEaten++;
                    Debug.Log($"���� {i} (12�� ��ó) ����");
                }
            }
        }

        // ���� ��� ���� ������ �Ŵ����� �˸�
        if (newSlicesEaten > 0 && CheckClear())
        {
            manager.OnDonutCleared();
        }
    }

    // ���� ��� ���� ���� üũ
    bool CheckClear()
    {
        int fullMask = (1 << sliceCount) - 1;
        return (eatenFlags & fullMask) == fullMask;
    }
}
