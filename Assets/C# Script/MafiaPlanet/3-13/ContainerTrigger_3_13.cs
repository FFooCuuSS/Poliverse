using System.Collections.Generic;
using UnityEngine;

// 3-13 케이스 칸. 판정은 CaseManager_3_13이 노래 시간으로 직접 한다.
// ※ 예전에는 칸마다 MiniGameBase라서 클릭 한 번에 칸 4개가 각자 판정/점수를 냈다 → 이제 겹침 정보만 갖는다.
public class ContainerTrigger_3_13 : MonoBehaviour
{
    [Header("Case Manager")]
    [SerializeField] private CaseManager_3_13 caseManager;

    [Header("이 컨테이너 종류")]
    [SerializeField]
    private CaseManager_3_13.ObjectType containerType;

    private readonly List<MovingObject_3_13> overlapObjects =
        new List<MovingObject_3_13>();

    public CaseManager_3_13.ObjectType ContainerType => containerType;

    // (구버전 호환) 데모는 CaseManager가 처리
    public void SetDemoMode(bool isDemo)
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MovingObject_3_13 obj = other.GetComponent<MovingObject_3_13>();
        if (obj == null) obj = other.GetComponentInParent<MovingObject_3_13>();
        if (obj == null) return;

        if (!overlapObjects.Contains(obj))
            overlapObjects.Add(obj);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MovingObject_3_13 obj = other.GetComponent<MovingObject_3_13>();
        if (obj == null) obj = other.GetComponentInParent<MovingObject_3_13>();
        if (obj == null) return;

        overlapObjects.Remove(obj);
    }

    public void ClearObject()
    {
        overlapObjects.Clear();
    }
}