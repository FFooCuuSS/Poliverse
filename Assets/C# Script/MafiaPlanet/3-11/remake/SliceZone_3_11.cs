using System.Collections.Generic;
using UnityEngine;

public class SliceZone_3_11 : MonoBehaviour
{
    // 현재 판정 구역 안에 있는 오브젝트
    private readonly List<FlyingObject_3_11> objectList =
        new List<FlyingObject_3_11>();

    /// <summary>
    /// 오브젝트가 판정 구역 안으로 들어왔을 때 호출된다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Target 또는 Bomb만 등록
        if (!other.CompareTag("Target") &&
            !other.CompareTag("Bomb"))
        {
            return;
        }

        FlyingObject_3_11 obj =
            other.GetComponent<FlyingObject_3_11>();

        // Collider가 자식에 붙어있는 경우
        if (obj == null)
        {
            obj = other.GetComponentInParent<FlyingObject_3_11>();
        }

        if (obj == null)
        {
            return;
        }

        if (!objectList.Contains(obj))
        {
            objectList.Add(obj);

            Debug.Log(
                "[3-11] 판정 구역 진입 : " +
                obj.name
            );
        }
    }

    /// <summary>
    /// 오브젝트가 판정 구역 밖으로 나갔을 때 호출된다.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        FlyingObject_3_11 obj =
            other.GetComponent<FlyingObject_3_11>();

        if (obj == null)
        {
            obj = other.GetComponentInParent<FlyingObject_3_11>();
        }

        if (obj == null)
        {
            return;
        }

        if (objectList.Contains(obj))
        {
            objectList.Remove(obj);

            Debug.Log(
                "[3-11] 판정 구역 이탈 : " +
                obj.name
            );
        }
    }

    /// <summary>
    /// 현재 판정 구역 안에서
    /// 가장 중앙에 가까운 오브젝트를 반환한다.
    /// </summary>
    public FlyingObject_3_11 GetClosestObject()
    {
        RemoveNullObject();

        if (objectList.Count == 0)
        {
            return null;
        }

        FlyingObject_3_11 closest = null;

        float minDistance = float.MaxValue;

        for (int i = 0; i < objectList.Count; i++)
        {
            FlyingObject_3_11 obj = objectList[i];

            if (obj == null)
            {
                continue;
            }

            if (obj.IsProcessed)
            {
                continue;
            }

            float distance =
                Vector2.SqrMagnitude(
                    obj.transform.position -
                    transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obj;
            }
        }

        return closest;
    }

    /// <summary>
    /// 처리된 오브젝트를 리스트에서 제거한다.
    /// </summary>
    public void RemoveObject(FlyingObject_3_11 obj)
    {
        if (obj == null)
        {
            return;
        }

        objectList.Remove(obj);
    }

    /// <summary>
    /// 게임 종료 시 리스트를 비운다.
    /// </summary>
    public void ClearZone()
    {
        objectList.Clear();
    }

    /// <summary>
    /// Destroy된 오브젝트를 정리한다.
    /// </summary>
    private void RemoveNullObject()
    {
        for (int i = objectList.Count - 1;
             i >= 0;
             i--)
        {
            if (objectList[i] == null)
            {
                objectList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 현재 판정 구역 안에 있는 오브젝트 개수
    /// </summary>
    public int GetObjectCount()
    {
        RemoveNullObject();

        return objectList.Count;
    }

    /// <summary>
    /// 디버그용
    /// </summary>
    public void PrintObjectList()
    {
        RemoveNullObject();

        Debug.Log(
            "[3-11] 현재 판정 구역 물체 개수 : " +
            objectList.Count
        );

        foreach (FlyingObject_3_11 obj in objectList)
        {
            if (obj != null)
            {
                Debug.Log(obj.name);
            }
        }
    }
}