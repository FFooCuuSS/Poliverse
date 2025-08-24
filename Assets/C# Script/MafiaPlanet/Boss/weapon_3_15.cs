using UnityEngine;

public class weapon_3_15 : MonoBehaviour
{
    [SerializeField] private bool isMagicWand = false; // ������ ����
    [SerializeField] private manager_3_15 manager;     // �ܺ� �Ŵ��� ����

    private void OnMouseDown()
    {
        // ������Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);

        // ���� �������̶�� manager�� �Լ� ����
        if (isMagicWand && manager != null)
        {
            manager.OnMagicWandCollected();
        }
    }
}
