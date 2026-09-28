using System.Collections.Generic;
using UnityEngine;

public class ContainerTrigger_3_13 : MiniGameBase
{

    [Header("Case Manager")]
    [SerializeField] private CaseManager_3_13 caseManager;

    [Header("이 컨테이너 종류")]
    [SerializeField]
    private CaseManager_3_13.ObjectType containerType;
    // 현재 컨테이너와 겹쳐있는 오브젝트
    private readonly List<MovingObject_3_13> overlapObjects =
        new List<MovingObject_3_13>();

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        CheckInput();
    }

    /// <summary>
    /// 좌클릭 시 현재 겹쳐있는 물체를 검사한다.
    /// </summary>
    private void CheckInput()
    {

        // 이 컨테이너와 겹친 물체가 없으면 아무 처리도 하지 않는다.
        if (overlapObjects.Count == 0)
        {
            base.ReportManualFail();
            return;
        }

        MovingObject_3_13 selectedObject = null;

        // 아직 처리되지 않은 물체 하나를 찾는다.
        for (int i = 0; i < overlapObjects.Count; i++)
        {
            MovingObject_3_13 currentObject =
                overlapObjects[i];

            if (currentObject == null ||
                currentObject.IsProcessed)
            {
                continue;
            }

            selectedObject = currentObject;
            break;
        }

        if (selectedObject == null)
        {
            return;
        }

        // 물체 종류와 컨테이너 종류가 같으면 성공
        if (selectedObject.ObjectType == containerType)
        {
            // 현재 Case에 맞는 성공 표시 오브젝트 활성화
            if (caseManager != null)
            {
                base.ReportManualSuccess();
                caseManager.ShowSuccessObject(
                    selectedObject.ObjectType
                );
            }
            else
            {
                Debug.LogWarning(
                    "[3-13] CaseManager가 연결되지 않았습니다."
                );
            }

            // Trigger 목록에서 먼저 제거
            overlapObjects.Remove(selectedObject);

            // MovingObject 쪽 성공 처리
            // 성공 로그 출력 후 이동 오브젝트 즉시 삭제
            selectedObject.ProcessSuccess();
        }
        else
        {
            base.ReportManualFail();
            // 잘못된 컨테이너에서 클릭한 경우
            selectedObject.ProcessWrongClick();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        MovingObject_3_13 obj =
            other.GetComponent<MovingObject_3_13>();

        if (obj == null)
        {
            obj =
                other.GetComponentInParent<MovingObject_3_13>();
        }

        if (obj == null)
        {
            return;
        }

        if (!overlapObjects.Contains(obj))
        {
            overlapObjects.Add(obj);

            Debug.Log(
                "[3-11] " +
                obj.ObjectType +
                " 컨테이너 진입"
            );
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MovingObject_3_13 obj =
            other.GetComponent<MovingObject_3_13>();

        if (obj == null)
        {
            obj =
                other.GetComponentInParent<MovingObject_3_13>();
        }

        if (obj == null)
        {
            return;
        }

        overlapObjects.Remove(obj);

        Debug.Log(
            "[3-13] " +
            obj.ObjectType +
            " 컨테이너 이탈"
        );
    }

    /// <summary>
    /// Destroy된 오브젝트 제거
    /// </summary>
    private void RemoveNullObject()
    {
        for (int i = overlapObjects.Count - 1;
             i >= 0;
             i--)
        {
            if (overlapObjects[i] == null)
            {
                overlapObjects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 게임 종료 시 호출
    /// </summary>
    public void ClearObject()
    {
        overlapObjects.Clear();
    }
}