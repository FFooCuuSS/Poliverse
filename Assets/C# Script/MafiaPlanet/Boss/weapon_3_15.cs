using UnityEngine;

public class weapon_3_15 : MonoBehaviour
{
    [SerializeField] private bool isMagicWand = false; // 마법봉 여부
    [SerializeField] private manager_3_15 manager;     // 외부 매니저 연결

    private void OnMouseDown()
    {
        // 오브젝트 비활성화
        gameObject.SetActive(false);

        // 만약 마법봉이라면 manager의 함수 실행
        if (isMagicWand && manager != null)
        {
            manager.OnMagicWandCollected();
        }
    }
}
