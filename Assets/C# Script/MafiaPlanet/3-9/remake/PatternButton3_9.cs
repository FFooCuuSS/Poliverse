using UnityEngine;

public class PatternButton3_9 : MonoBehaviour
{
    [Header("3-9 Manager")]
    public Manager3_9 manager;

    [Header("패턴 번호")]
    [Tooltip("pattern1 = 0, pattern2 = 1 ... pattern6 = 5")]
    public int slotIndex;


    /// <summary>
    /// Collider2D가 붙어있는 오브젝트를
    /// 마우스로 클릭했을 때 호출된다.
    /// </summary>
    private void OnMouseDown()
    {
        // 클릭 확인용 로그
        Debug.Log(
            "[3-9] Pattern 클릭 : " +
            (slotIndex + 1)
        );


        // Manager가 연결되어 있는지 확인
        if (manager == null)
        {
            Debug.LogError(
                "[3-9] Manager3_9가 연결되지 않음"
            );

            return;
        }


        // 클릭한 패턴 번호를 Manager에게 전달
        manager.OnPatternClicked(
            slotIndex
        );
    }
}